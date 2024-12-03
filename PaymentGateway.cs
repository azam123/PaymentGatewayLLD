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

