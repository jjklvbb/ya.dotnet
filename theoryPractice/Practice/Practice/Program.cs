var users = await db.Users.Include(u => u.Orders);

var result = users
    .Where(u => u.Country == "Япония")
    .Where(u => u.LastActiveAtUtc >= DateTime.UtcNow.AddDays(-14))
    .Select(u => new
    {
        u.Id,
        u.Gender,
        u.LastActiveAtUtc,
        u.Country,
        SpendLastThreeMonths = u.Orders
            .Where(o => o.PaidAtUtc >= DateTime.UtcNow.AddMonths(-3))
            .Sum(o => o.TotalAmount)
    })
    .Where(x => x.SpendLastThreeMonths >= 300000)
    .GroupBy(x => x.Gender)
    .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());