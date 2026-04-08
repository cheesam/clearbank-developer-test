# ClearBank Developer Test: PaymentService Refactor

## 1. Problem analysis

`PaymentService` has one public method and no constructor. Everything is crammed into `MakePayment`.

**`ConfigurationManager.AppSettings` read inline (line 11)**

`ConfigurationManager.AppSettings["DataStoreType"]` is called directly in the method body. I can't inject it, swap it, or mock it. It reads from the running process's config file. The method owns its own environment resolution, so there's no way to test it in isolation.

**Data store instantiated twice (lines 17–23 and 78–86)**

The store selection block appears twice: once to call `GetAccount`, once to call `UpdateAccount`. Two separate instances, both resolved from config. The intent is obviously to read and write to the same store, but nothing in the code actually guarantees that. If the two blocks ever diverged, you'd be reading from one store and writing to another with no indication anything had gone wrong.

**Null check repeated in every switch case (lines 33, 44, 59)**

Each `case` block opens with `if (account == null) { result.Success = false; }`. Same guard, three times, none handling it differently. One change needed in one place is actually three changes needed in three places.

**Validation living inside `PaymentService` (lines 30–72)**

The switch statement means `PaymentService` contains the specific rules for Bacs, FasterPayments, and Chaps. The service should be orchestrating, not adjudicating. And when a fourth scheme gets added, someone has to open this file and edit it.

**Nothing is injected**

No constructor. The config reader and both data stores are instantiated inside the method body. There's no seam anywhere for a test double. The class simply cannot be unit tested as written.

---

## 2. SOLID: what it means for this problem

The one that bites hardest is **dependency inversion**. `PaymentService` currently depends directly on `ConfigurationManager`, `AccountDataStore`, and `BackupAccountDataStore`, all concrete, all hardwired. Nothing is abstracted, so there's nothing to swap in a test. After the refactor, the service depends on `IAccountDataStore` and `IPaymentValidatorFactory`. It doesn't know or care what's behind either.

**Open/closed** is the other one that really matters here. Adding a new payment scheme means editing `MakePayment`. That's the definition of a class that isn't closed to modification. Pulling validation into per-scheme classes and selecting them via a factory means a new scheme is a new file. `PaymentService` doesn't change.

**Single responsibility** falls into place once the above are fixed. With validation extracted and the data store injected, `MakePayment` is left doing three things: get the account, validate, update the balance. That's a reasonable scope for a service method.

**Liskov substitution** is addressed by extracting `IAccountDataStore`. The two concrete stores already have identical signatures but don't share an abstraction. Once they do, either should be passable wherever the interface is expected.

**Interface segregation** isn't really a concern here. The interfaces I'm extracting are narrow by nature.

---

## 3. Architectural decisions

**Extract `IAccountDataStore`**

Both stores already have matching method signatures, so this is just formalising what's already true. Minimum change to make the data store injectable; which store gets used becomes a decision for the composition root, not for `PaymentService`.

**One `IPaymentValidator` per scheme**

`BacsPaymentValidator`, `FasterPaymentsPaymentValidator`, `ChapsPaymentValidator`: each knows the rules for exactly one scheme, and `PaymentService` knows nothing about any of them. New scheme, new class. Nothing existing changes.

**`PaymentValidatorFactory` using a dictionary**

The factory holds a `Dictionary<PaymentScheme, IPaymentValidator>` populated at construction. `GetValidator` is a lookup. No switch statement, no fall-through risk, injected as `IPaymentValidatorFactory` so it can be mocked in tests. If a scheme isn't registered the factory returns null and the service treats it as a failure, which matches how the original handles an unrecognised scheme.

**Constructor injection throughout**

`PaymentService` takes `IAccountDataStore` and `IPaymentValidatorFactory` in its constructor. Nothing is instantiated inside the class.

**`PaymentValidatorFactory` accepts `IDictionary<PaymentScheme, IPaymentValidator>`**

The dictionary is passed in rather than built internally. The factory has no knowledge of which validators exist; the caller owns the wiring. This keeps the factory open for extension without any changes to the class itself.

**`[Flags]` on `AllowedPaymentSchemes`**

The enum was already used as a flags enum (bitwise values, `HasFlag` checks) but was missing the attribute. Added for correctness — it affects serialisation behaviour and `ToString()` output, and makes the intent explicit.

**Input guards on `MakePayment`**

A null request throws `ArgumentNullException`. A non-positive amount returns failure immediately. Both are boundary checks at the entry point of the method.

**`Account.Debit`**

Balance mutation moved into the domain object. `Debit` throws `InvalidOperationException` if the amount exceeds the balance, giving `Account` some self-protection rather than leaving that concern to the caller.

**No Clean Architecture**

In a production service I'd want a proper domain/application/infrastructure split. For an exercise with one service class, that layering adds more overhead than it demonstrates. I didn't want the architecture to become the story when the SOLID work is the actual point.

**No Autofac**

I use it day to day. Wiring it up here would add noise without adding anything instructive. Constructor injection is what I'm demonstrating; what resolves the graph is beside the point for this exercise.

---

## 4. Test scenarios

The intention was to write characterisation tests against the original `PaymentService` first, then refactor, then confirm they still pass. In practice the original code makes that only partially possible.

Both data stores return `new Account()` with default values — no flags set, balance zero. There's no way to inject an account in any other state without changing production code. That means the null-account case is unreachable (the store never returns null), and the happy paths are unreachable (the scheme flag will never be set). The three failure cases caused by missing flags are the only paths observable from outside the class.

The scenarios below are the full intended suite. Those marked can be written against the original; the rest require injection and are written after the refactor.

**Bacs**
- Bacs flag not set → failure ✓ characterisation
- Account is null → failure (requires injection)
- Account exists, Bacs flag set → success, balance decremented (requires injection)

**FasterPayments**
- FasterPayments flag not set → failure ✓ characterisation
- Account is null → failure (requires injection)
- Balance less than payment amount → failure (requires injection)
- Account exists, flag set, balance sufficient → success, balance decremented (requires injection)

**Chaps**
- Chaps flag not set → failure ✓ characterisation
- Account is null → failure (requires injection)
- Account status not Live → failure (requires injection)
- Account exists, flag set, status is Live → success, balance decremented (requires injection)

The balance-deduction logic has no characterisation coverage at all. If that code were broken during the refactor, nothing would catch it until the post-refactor unit tests are written. That's a known gap, not an oversight.

**Input guards**
- Null request → throws `ArgumentNullException`
- Zero amount → failure
- Negative amount → failure

**`Account.Debit`**
- Valid amount → balance decremented
- Amount exceeds balance → throws `InvalidOperationException`

**`PaymentValidatorFactory`**
- Bacs scheme → returns `BacsPaymentValidator`
- FasterPayments scheme → returns `FasterPaymentsPaymentValidator`
- Chaps scheme → returns `ChapsPaymentValidator`
- Unregistered scheme → returns null
