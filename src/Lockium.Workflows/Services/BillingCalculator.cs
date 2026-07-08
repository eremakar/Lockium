namespace Lockium.Workflows.Services
{
    public class BillingCalculator : IBillingCalculator
    {
        public const double FixedAmount = 100;

        public double CalculateAmount() => FixedAmount;
    }
}
