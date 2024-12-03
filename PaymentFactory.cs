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
