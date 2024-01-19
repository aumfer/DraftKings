class Customer
{
    public int CustomerId { get; set; }
    public decimal YearlySpend { get; set; }
}

class Discount
{
    public int DiscountId { get; set; }
    public decimal DiscountValue { get; set; }
}

class CustomerDiscount
{
    public int CustomerId { get; set; }
    public int DiscountId { get; set; }
}

class DK
{
    public static bool ValidateCustomerDiscounts(ICollection<Customer> customers, ICollection<Discount> discounts, ICollection<CustomerDiscount> customerDiscounts)
    {
        if (customers == null) throw new ArgumentNullException(nameof(customers));
        if (discounts == null) throw new ArgumentNullException(nameof(discounts));
        if (customerDiscounts == null) throw new ArgumentNullException(nameof(customerDiscounts));

        var customersWithDiscounts = (
            from c in customers
            join cd in customerDiscounts on c.CustomerId equals cd.CustomerId
            join d in discounts on cd.DiscountId equals d.DiscountId into discountGroup
            let discountsList = discountGroup.ToList()
            let totalDiscountValue = discountsList.Sum(d => d.DiscountValue)
            select new
            {
                Customer = c,
                Discounts = discountsList,
                TotalDiscountValue = totalDiscountValue
            }
        ).ToList();

        var noTooManyDiscounts = customersWithDiscounts.All(cd => cd.Discounts.Count < 3);
        var noExcessDiscountValue = customersWithDiscounts.All(cd => cd.TotalDiscountValue < cd.Customer.YearlySpend * 0.2m);
        var orphanedDiscounts = from d in discounts
                                join cd in customerDiscounts on d.DiscountId equals cd.DiscountId
                                join c in customers on cd.CustomerId equals c.CustomerId into discountedCustomer
                                where !discountedCustomer.Any()
                                select d.DiscountId;
        var noOrphanedDiscounts = !orphanedDiscounts.Any();
        var customersByDiscountValue = customersWithDiscounts.OrderBy(cd => cd.TotalDiscountValue).ToList();
        var customersByYearlySpend = customersWithDiscounts.OrderBy(cd => cd.Customer.YearlySpend).ToList();
        var req4 = customersByDiscountValue.SequenceEqual(customersByYearlySpend);
        var validDiscounts = noTooManyDiscounts && noExcessDiscountValue && noOrphanedDiscounts && req4;
        return validDiscounts;
    }
}
