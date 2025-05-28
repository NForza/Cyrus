

using System.Collections.Generic;

List<Investor> investors = new List<Investor>()
{
    new Investor{ Name ="A", CapitalToInvest = 1000 },
    new Investor{ Name ="B",  CapitalToInvest = 2000 },
    new Investor{ Name ="C", CapitalToInvest = 10000 },
    new Investor{ Name ="D",  CapitalToInvest = 2000 },
};


List<Startup> startups = new List<Startup>()
{
    new Startup{ Name ="A", NeededCapital = 1000 },
    new Startup{ Name ="B",  NeededCapital = 2000 },
    new Startup{ Name ="C", NeededCapital = 1000 },
    new Startup{ Name ="D",  NeededCapital = 2000 },
    new Startup{ Name ="E",  NeededCapital = 3000 },
};



IEnumerable<(Startup, IEnumerable<Investor>)> SelectInvestor(IEnumerable<(Startup, IEnumerable<Investor>)> startups, List<Investor> investors, IEnumerable<Startup> startups)
{
    var startup = startups.FirstOrDefault();
    if (startup.Any())
        return startups;

    List<(Startup startup, IEnumerable<Investor> investors)> result = [];
    int investorToStart = 0;
    var neededCapital = startup.NeededCapital;
    double amountCollected = 0;
    List<Investor> selectedInvestors = [];
    for (var i = investorToStart; i < investors.Count; i++)
    {
        var investor = investors[i];
        if (investor.CapitalToInvest > 0)
        {
            amountCollected += investor.CapitalToInvest;
            investor.CapitalToInvest -= amountCollected;
            selectedInvestors.Add(investor);
        }
        if (amountCollected >= neededCapital)
        {
            if (amountCollected > neededCapital)
            {
                var overCollectedCapital = amountCollected - neededCapital;
                investor.CapitalToInvest += amountCollected;
            }
            break;
        }
    }

    return SelectInvestor(startups, ;
}

var results = SelectInvestor(investors, startups);
foreach (var item in results)
{
    Console.WriteLine(item.Item1.ToString());
    Console.WriteLine(item.Item2.ToString());
}

class Investor
{
    public string Name { get; set; }
    public double CapitalToInvest { get; set; }
}

class Startup
{
    public string Name { get; set; }
    public double NeededCapital { get; set; }
}

