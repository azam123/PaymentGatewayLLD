### **Payment Gateway Low-Level Design**

---

#### **Requirements**

1. **Functional Requirements**:
   - Handle multiple payment methods: 
     - UPI (Unified Payments Interface)
     - Online Banking
     - Credit/Debit Cards
   - Validate payment details for each payment method.
   - Process payments securely.
   - Handle payment status updates (Success, Failure, Pending).
   - Provide a retry mechanism for failed transactions.
   - Support asynchronous notification for payment status.
   - Allow reconciliation for payments.

2. **Non-Functional Requirements**:
   - **Scalability**: Handle high transaction volume.
   - **Availability**: Ensure high uptime.
   - **Security**: Use secure protocols (e.g., HTTPS, encryption for sensitive data).
   - **Performance**: Process payments with low latency.
   - **Auditability**: Maintain transaction logs.
   - **Extensibility**: Allow adding new payment methods in the future.

---

#### **UML Diagrams**

1. **Use Case Diagram**
   - Actors: User, Payment Gateway, Bank/UPI/Card Network.
   - Use Cases:
     - Select Payment Method
     - Validate Payment
     - Process Payment
     - Notify Payment Status
     - Retry Failed Payment

2. **Class Diagram**
   - Key classes:
     - `PaymentGateway`
     - `PaymentProcessor`
     - `Transaction`
     - `PaymentMethod` (Abstract)
     - `UPI`, `OnlineBanking`, `CardPayment` (Inherit from `PaymentMethod`)
     - `RetryHandler`
     - `NotificationService`
     - `Logger`

---

#### **Class Diagram**

```plaintext
+------------------+
| PaymentGateway   |
+------------------+
| ProcessPayment() |
| RetryPayment()   |
| NotifyStatus()   |
+------------------+
        ^
        |
+------------------+
| PaymentMethod    |<----------------+
+------------------+                 |
| Validate()       |                 |
| Authorize()      |                 |
| Process()        |                 |
+------------------+                 |
    ^        ^        ^              |
    |        |        |              |
+---+    +---+    +---+              |
| UPI |  |Bank|  |Card|              |
+---+    +---+    +---+              |
                                      |
+------------------+                  |
| RetryHandler     |                  |
+------------------+                  |
| Retry()          |-----------------+
+------------------+
```

---

#### **Database Schema**

1. **Transactions Table**
   ```sql
   CREATE TABLE Transactions (
       TransactionID VARCHAR(36) PRIMARY KEY,
       UserID VARCHAR(36),
       PaymentMethod VARCHAR(20),
       Amount DECIMAL(10, 2),
       Status VARCHAR(20),
       CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
       UpdatedAt TIMESTAMP
   );
   ```

2. **PaymentMethods Table**
   ```sql
   CREATE TABLE PaymentMethods (
       MethodID VARCHAR(36) PRIMARY KEY,
       UserID VARCHAR(36),
       MethodType VARCHAR(20),
       Details JSON,
       CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
   );
   ```

3. **Logs Table**
   ```sql
   CREATE TABLE Logs (
       LogID INT AUTO_INCREMENT PRIMARY KEY,
       TransactionID VARCHAR(36),
       Message TEXT,
       LogLevel VARCHAR(10),
       CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
   );
   ```

---

#### **Retry Mechanism**

- **Retry Policy**:
  - Retry failed payments up to 3 times with exponential backoff.
  - Use a queue system to handle retries asynchronously.

---

#### **Code (C#)**

```csharp
using System;

namespace PaymentGateway
{
    // Abstract class for Payment Methods
    public abstract class PaymentMethod
    {
        public abstract bool Validate();
        public abstract bool Authorize();
        public abstract bool Process();
    }

    // UPI Payment Implementation
    public class UpiPayment : PaymentMethod
    {
        public override bool Validate() { /* Validation logic */ return true; }
        public override bool Authorize() { /* Authorization logic */ return true; }
        public override bool Process() { /* Processing logic */ return true; }
    }

    // Credit/Debit Card Payment Implementation
    public class CardPayment : PaymentMethod
    {
        public override bool Validate() { /* Validation logic */ return true; }
        public override bool Authorize() { /* Authorization logic */ return true; }
        public override bool Process() { /* Processing logic */ return true; }
    }

    // Payment Gateway
    public class PaymentGateway
    {
        public void ProcessPayment(PaymentMethod method)
        {
            if (method.Validate() && method.Authorize())
            {
                if (method.Process())
                    Console.WriteLine("Payment Processed Successfully!");
                else
                    Console.WriteLine("Payment Processing Failed!");
            }
            else
            {
                Console.WriteLine("Payment Validation or Authorization Failed!");
            }
        }
    }

    // Retry Handler
    public class RetryHandler
    {
        private const int MaxRetryCount = 3;

        public void Retry(Func<bool> operation)
        {
            int attempts = 0;
            while (attempts < MaxRetryCount)
            {
                if (operation())
                {
                    Console.WriteLine("Operation succeeded.");
                    return;
                }
                attempts++;
                System.Threading.Thread.Sleep(1000 * attempts); // Exponential Backoff
            }
            Console.WriteLine("Operation failed after retries.");
        }
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            PaymentGateway gateway = new PaymentGateway();

            // Example: Process UPI Payment
            PaymentMethod upi = new UpiPayment();
            gateway.ProcessPayment(upi);

            // Example: Retry Mechanism
            RetryHandler retryHandler = new RetryHandler();
            retryHandler.Retry(() => { /* Retry Logic */ return false; });
        }
    }
}
```

---

#### **Design Patterns**

1. **Factory Pattern**:
   - Used to instantiate specific payment method objects (`UPI`, `CardPayment`, etc.) dynamically.

   **Example**:
   ```csharp
   public class PaymentFactory
   {
       public static PaymentMethod CreatePaymentMethod(string type)
       {
           return type switch
           {
               "UPI" => new UpiPayment(),
               "Card" => new CardPayment(),
               _ => throw new NotSupportedException("Payment method not supported")
           };
       }
   }
   ```

2. **Retry Pattern**:
   - Handles retries with exponential backoff logic.
   - 


Enhancement with Strategy Pattern.

To avoid tight dependency and support adding new payment methods in the future, we can use the **Strategy Pattern** instead of the Factory Pattern. The **Strategy Pattern** encapsulates different algorithms or behaviors (in this case, payment methods) and allows them to be interchangeable without modifying the client code. This approach adheres to the **Open/Closed Principle**, enabling new payment methods to be added with minimal changes.

---

### **Updated Design Using Strategy Pattern**

#### **Key Changes**

1. Define a `PaymentStrategy` interface for all payment methods.
2. Each payment method implements the `PaymentStrategy` interface.
3. The `PaymentGateway` class dynamically uses the appropriate payment strategy.

---

#### **Class Diagram**

```plaintext
+------------------+
| PaymentGateway   |
+------------------+
| SetStrategy()    |
| ProcessPayment() |
+------------------+
        |
        v
+------------------+
| PaymentStrategy  |<---------------------------------------+
+------------------+                                        |
| Validate()       |                                        |
| Authorize()      |                                        |
| Process()        |                                        |
+------------------+                                        |
    ^            ^            ^                             |
    |            |            |                             |
+-------+    +-------+    +-------+                         |
|  UPI  |    | Card  |    | Bank  |                         |
+-------+    +-------+    +-------+                         |
                                                           |
+------------------+                                       |
| AddNewMethod     |                                       |
+------------------+                                       |
| Implement        |--------------------------------------+
```

---

#### **Code Implementation**

```csharp
using System;

namespace PaymentGatewayWithStrategy
{
    // Strategy Interface
    public interface IPaymentStrategy
    {
        bool Validate();
        bool Authorize();
        bool Process();
    }

    // Concrete Strategy: UPI Payment
    public class UpiPayment : IPaymentStrategy
    {
        public bool Validate()
        {
            Console.WriteLine("Validating UPI details...");
            return true;
        }

        public bool Authorize()
        {
            Console.WriteLine("Authorizing UPI transaction...");
            return true;
        }

        public bool Process()
        {
            Console.WriteLine("Processing UPI payment...");
            return true;
        }
    }

    // Concrete Strategy: Card Payment
    public class CardPayment : IPaymentStrategy
    {
        public bool Validate()
        {
            Console.WriteLine("Validating Card details...");
            return true;
        }

        public bool Authorize()
        {
            Console.WriteLine("Authorizing Card transaction...");
            return true;
        }

        public bool Process()
        {
            Console.WriteLine("Processing Card payment...");
            return true;
        }
    }

    // Concrete Strategy: Online Banking
    public class OnlineBankingPayment : IPaymentStrategy
    {
        public bool Validate()
        {
            Console.WriteLine("Validating Online Banking details...");
            return true;
        }

        public bool Authorize()
        {
            Console.WriteLine("Authorizing Online Banking transaction...");
            return true;
        }

        public bool Process()
        {
            Console.WriteLine("Processing Online Banking payment...");
            return true;
        }
    }

    // Context: Payment Gateway
    public class PaymentGateway
    {
        private IPaymentStrategy _paymentStrategy;

        public void SetStrategy(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public void ProcessPayment()
        {
            if (_paymentStrategy == null)
            {
                Console.WriteLine("Payment method not set.");
                return;
            }

            if (_paymentStrategy.Validate() && _paymentStrategy.Authorize())
            {
                if (_paymentStrategy.Process())
                {
                    Console.WriteLine("Payment Processed Successfully!");
                }
                else
                {
                    Console.WriteLine("Payment Processing Failed!");
                }
            }
            else
            {
                Console.WriteLine("Payment Validation or Authorization Failed!");
            }
        }
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            PaymentGateway gateway = new PaymentGateway();

            // Example: UPI Payment
            Console.WriteLine("Processing UPI Payment:");
            gateway.SetStrategy(new UpiPayment());
            gateway.ProcessPayment();

            // Example: Card Payment
            Console.WriteLine("\nProcessing Card Payment:");
            gateway.SetStrategy(new CardPayment());
            gateway.ProcessPayment();

            // Example: Online Banking Payment
            Console.WriteLine("\nProcessing Online Banking Payment:");
            gateway.SetStrategy(new OnlineBankingPayment());
            gateway.ProcessPayment();
        }
    }
}
```

---

### **Advantages of Strategy Pattern**

1. **Decoupling**:
   - The `PaymentGateway` class does not depend on specific payment method implementations.
   - New payment methods can be added without changing the existing code.

2. **Flexibility**:
   - The `SetStrategy` method allows dynamic switching between payment methods at runtime.

3. **Adherence to SOLID Principles**:
   - **Open/Closed Principle**: New payment methods can be added without modifying the `PaymentGateway` class.
   - **Single Responsibility Principle**: Each payment method has its own implementation, keeping the code clean.

---

### **Adding a New Payment Method**

For example, if a new method like **"CryptoPayment"** needs to be supported:

1. Implement the `IPaymentStrategy` interface:
   ```csharp
   public class CryptoPayment : IPaymentStrategy
   {
       public bool Validate()
       {
           Console.WriteLine("Validating Crypto wallet...");
           return true;
       }

       public bool Authorize()
       {
           Console.WriteLine("Authorizing Crypto transaction...");
           return true;
       }

       public bool Process()
       {
           Console.WriteLine("Processing Crypto payment...");
           return true;
       }
   }
   ```

2. Use it in the `PaymentGateway`:
   ```csharp
   gateway.SetStrategy(new CryptoPayment());
   gateway.ProcessPayment();
   ```

This approach ensures that adding new payment methods is seamless and does not require modifying the existing classes.

   

3. **Architectural Pattern**:
   - **Microservices**:
     - Divide the payment gateway into smaller services (e.g., Payment Service, Notification Service, Retry Service).
     - Benefits: Scalability, Fault Isolation.
     - Use REST APIs for communication.

   - **Event-Driven Architecture**:
     - Use messaging systems (e.g., RabbitMQ) for asynchronous processing (e.g., retries, notifications).

   **Example**: Payment gateway communicates with external payment providers via REST APIs while handling retries via an internal message queue.
