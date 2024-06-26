namespace Pix.Models
{
    public class PaymentIdempotenceKey(Payment payment)
    {
        public int PixKeyId { get; } = payment.PixKeyId;
        public int PaymentProviderAccountId { get; } = payment.PaymentProviderAccountId;
        public int Amount { get; } = payment.Amount;
        public string? Description { get; } = payment.Description;
    }
}
