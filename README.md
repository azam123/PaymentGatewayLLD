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

3. **Architectural Pattern**:
   - **Microservices**:
     - Divide the payment gateway into smaller services (e.g., Payment Service, Notification Service, Retry Service).
     - Benefits: Scalability, Fault Isolation.
     - Use REST APIs for communication.

   - **Event-Driven Architecture**:
     - Use messaging systems (e.g., RabbitMQ) for asynchronous processing (e.g., retries, notifications).

   **Example**: Payment gateway communicates with external payment providers via REST APIs while handling retries via an internal message queue.
