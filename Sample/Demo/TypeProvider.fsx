// ======================= RegEx =================================
// http://fsprojects.github.io/FSharp.Text.RegexProvider/
#I "../packages/FSharp.Text.RegexProvider/lib/net40"
#r "FSharp.Text.RegexProvider.dll"
open FSharp.Text.RegexProvider

// Let the type provider do its work
type PhoneRegex = Regex< @"(?<AreaCode>^\d{3})-(?<PhoneNumber>\d{3}-\d{4}$)" >


// Regex expression from http://emailregex.com/ (C# version)
type EmailRegex = Regex< @"^(?<UserId>(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@)))(?<Domain>(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9])))$">

// now you have typed access to the regex groups and you can browse it via Intellisense
PhoneRegex().TypedMatch("425-123-2345").AreaCode.Value

EmailRegex().TypedMatch("dmitry.batalov@fastdev.se").UserId.Value


// ======================= CSV =================================
// http://fsharp.github.io/FSharp.Data/library/CsvProvider.html
#I "../packages/FSharp.Data/lib/net45"
#r "FSharp.Data.dll"
open FSharp.Data

type Stocks = CsvProvider<"http://www.google.com/finance/historical?q=MSFT&output=csv">

let msft = Stocks.Load("http://www.google.com/finance/historical?q=MSFT&output=csv")
let firstRow = msft.Rows |> Seq.head
let lastDate = firstRow.Date
let lastOpen = firstRow.Open



// ======================= Twitter =================================
// http://fsprojects.github.io/FSharp.Data.Toolbox/TwitterProvider.html
// First, reference the locations where F# Data and 
// F# Data Toolbox are located (using '#I' is required here!)
#I @"../packages/FSharp.Data/lib/net45"
#I @"../packages/FSharp.Data.Toolbox.Twitter/lib/net40"

// The Twitter reference needs to come before FSharp.Data.dll
// (see the big warning box below for more!)
#r "FSharp.Data.dll"
#r "FSharp.Data.Toolbox.Twitter.dll"
open FSharp.Data.Toolbox.Twitter

let key = "mKQL29XNemjQbLlQ8t0pBg"
let secret = "T27HLDve1lumQykBUgYAbcEkbDrjBe6gwbu0gqi4saM"
let twitter = Twitter.AuthenticateAppOnly(key, secret)
// let fsharpTweets = twitter.Search.Tweets("#fsharp", count=2)

for status in twitter.Search.Tweets("#fsharp", count=2).Statuses do
  printfn "@%s: %s" status.User.ScreenName status.Text


// ======================= HTTP =================================  

#I @"../packages/FSharp.Data/lib/net45"
#r "FSharp.Data.dll"

open FSharp.Data
// Configure the type provider
type NugetStats = 
  HtmlProvider<"https://www.nuget.org/packages/FSharp.Data">

// load the live package stats for FSharp.Data
let rawStats = NugetStats().Tables.``Version History``

// helper function to analyze version numbers from nuget
let getMinorVersion (v:string) =  
  System.Text.RegularExpressions.Regex(@"\d.\d").Match(v).Value

// group by minor version and calculate download count
let stats = 
  rawStats.Rows
  |> Seq.groupBy (fun r -> 
      getMinorVersion r.Version)
  |> Seq.map (fun (k, xs) -> 
      k, xs |> Seq.sumBy (fun x -> x.Downloads))

// Load the FSharp.Charting library
#load "../packages/FSharp.Charting/lib/net45/FSharp.Charting.fsx"
open FSharp.Charting

// Visualize the package stats
Chart.Bar stats

// -----------------------------------------------

type FSharpHistory = 
  HtmlProvider<"https://en.wikipedia.org/wiki/F_Sharp_(programming_language)">
 
let f = FSharpHistory().Tables.Versions
let versions = 
        f.Rows 
        |> Seq.map (fun x -> x.Version)
        |> Seq.toArray

printfn "Versions: %A" versions












// ======================= SQL =================================  
// https://fsprojects.github.io/SQLProvider/index.html
// reference the type provider dll
#I "../packages/SQLProvider/lib/net451"
#I "../packages/System.Data.SQLite.Core/lib/net451/"
#r "FSharp.Data.SQLProvider.dll"
#r "System.Data.SQLite.dll"
open FSharp.Data.Sql

let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + @"/../packages/System.Data.SQLite.Core/lib/net451/" 
let [<Literal>] connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"\db\chinook.db;Version=3;Read Only=false;FailIfMissing=True;"
// create a type alias with the connection string and database vendor settings
type sql = SqlDataProvider< 
              ConnectionString = connectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
              ResolutionPath = resolutionPath,
              IndividualsAmount = 1000,
              UseOptionTypes = true >
let ctx = sql.GetDataContext()

// To use dynamic runtime connectionString, you could use:
// let ctx = sql.GetDataContext connectionString2

// pick individual entities from the database 
let luis = ctx.Main.Customers.Individuals.``As Address``.``1, Av. Brigadeiro Faria Lima, 2170``

// directly enumerate an entity's relationships, 
// this creates and triggers the relevant query in the background
let luisOrders = luis.``main.invoices by CustomerId`` |> Seq.toArray


let leonieInvoiceDetails =
    query { for c in ctx.Main.Customers do
            // you can directly enumerate relationships with no join information
            for i in c.``main.invoices by CustomerId`` do
            // or you can explicitly join on the fields you choose
            join ii in ctx.Main.InvoiceItems on (i.InvoiceId = ii.InvoiceId)
            //  the (!!) operator will perform an outer join on a relationship
            for track in (!!) ii.``main.tracks by TrackId`` do
            // nullable columns can be represented as option types; the following generates IS NOT NULL
            where c.City.IsSome                
            // standard operators will work as expected; the following shows the like operator and IN operator
            where (c.FirstName =% ("Leon%") && 
                    i.BillingCity.IsSome && 
                    i.BillingCity.Value |=| [|"Stuttgart"; "Toronto"|] )
            sortBy i.InvoiceDate
            take 5
            // arbitrarily complex projections are supported
            select (c.FirstName,c.LastName,i.BillingCity, i.InvoiceDate, track.Name) } 
    |> Seq.toArray
