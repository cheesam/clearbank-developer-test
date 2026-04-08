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

**No Clean Architecture**

In a production service I'd want a proper domain/application/infrastructure split. For an exercise with one service class, that layering adds more overhead than it demonstrates. I didn't want the architecture to become the story when the SOLID work is the actual point.

**No Autofac**

I use it day to day. Wiring it up here would add noise without adding anything instructive. Constructor injection is what I'm demonstrating; what resolves the graph is beside the point for this exercise.

---

## 4. Test scenarios

Characterisation tests first, written against the original `PaymentService` to pin every execution path before I touch any production code.

**Bacs**
- Account exists, Bacs flag set → success, balance decremented
- Account is null → failure
- Bacs flag not set → failure

**FasterPayments**
- Account exists, flag set, balance sufficient → success, balance decremented
- Account is null → failure
- FasterPayments flag not set → failure
- Balance less than payment amount → failure

**Chaps**
- Account exists, flag set, status is Live → success, balance decremented
- Account is null → failure
- Chaps flag not set → failure
- Account status not Live → failure 
