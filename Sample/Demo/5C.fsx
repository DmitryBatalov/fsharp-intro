

// let (|>) x f = f x

// ===================== Conciseness ======================
// one-liners
[1..100] |> List.sum |> printfn "sum=%d"

// no curly braces, semicolons or parentheses
let square x = x * x
let sq = square 42 

// simple types in one line
type Person = {First:string; Last:string}

// complex types in a few lines
type Employee = 
  | Worker of Person
  | Manager of Employee list

// type inference
let jdoe = {First="John";Last="Doe"}
let worker = Worker jdoe
let manager = Manager [worker]























// ================== Convenience =====================
// automatic equality and comparison
type Person = {First:string; Last:string}
let person1 = {First="john"; Last="Doe"}
let person2 = {First="john"; Last="Doe"}
printfn "Equal? %A"  (person1 = person2)

// easy composition of functions
let add2times3 = (+) 2 >> (*) 3
let result = add2times3 5


// Domain modeling 
type Suit = Club | Diamond | Spade | Heart
type Rank = Two   | Three | Four | Five | Six |
                Seven | Eight | Nine | Ten  |
                Jack  | Queen | King | Ace
type Card = Suit * Rank
type Hand = Card list
type Deck = Card list
type Player = { Name:string; Hand:Hand}
type Game = {Deck:Deck; Players: Player list}
type Deal = Deck -> (Deck*Card)
type PickupCard = (Hand*Card) -> Hand


// Fuctions as building blocks
// building blocks
let add2 x = x + 2
let mult3 x = x * 3
let square x = x * x

// helper functions;
let logMsg msg x = printf "%s%i" msg x; x     //without linefeed
let logMsgN msg x = printfn "%s%i" msg x; x   //with linefeed

let listOfFunctions = [mult3; square; add2; logMsgN "result=";]

// compose all functions in the list into a single one
let allFunctions = List.reduce (>>) listOfFunctions

//test
allFunctions 5






















// =================== Correctness =================
// strict type checking
printfn "print string %s" 123 //compile error

let person1 = {First="john"; Last="Doe"}
// all values immutable by default
person1.First <- "new name"  //assignment error 






// Designing for correctness
type CartItem = string    // placeholder for a more complicated type

type EmptyState = NoItems // don't use empty list! We want to
                        // force clients to handle this as a 
                        // separate case. E.g. "you have no 
                        // items in your cart"

type ActiveState = { UnpaidItems : CartItem list; }
type PaidForState = { PaidItems : CartItem list; 
                    Payment : decimal}

type Cart =
    | Empty of EmptyState
    | Active of ActiveState
    | PaidFor of PaidForState





// NullReference
// C# code
// string s1 = "abc";
// var len1 = s1.Length;

// string s2 = null;
// var len2 = s2.Length;

// F# code
let s1 = "abc"
let len1 = s1.Length

// create a string option with value None
let s2 = Option<string>.None
let len2 = s2.Length    // <- Compiler fails

let s2 = Option<string>.None
//which one is it?
let len2 = match s2 with
| Some s -> s.Length
| None -> 0





// units of measure
[<Measure>] type m
[<Measure>] type sec
[<Measure>] type kg

let distance = 1.0<m>
let time = 2.0<sec>
let speed = 2.0<m/sec>
let acceleration = 2.0<m/sec^2>
let force = 5.0<kg m/sec^2>

// check conversion correctness with measures
[<Measure>] type degC
[<Measure>] type degF

let convertDegCToF c = c * 1.8<degF/degC> + 32.0<degF>

let f = convertDegCToF 0.0<degC>












// ================== Concurrency =================
// easy async logic with "async" keyword
open Microsoft.FSharp.Control.CommonExtensions   // adds AsyncGetResponse
open System.Net
open System
open System.IO

// Fetch the contents of a web page asynchronously
let fetchUrlAsync url =        
    async {                             
        let req = WebRequest.Create(Uri(url)) 
        use! resp = req.AsyncGetResponse()  // new keyword "use!"  
        use stream = resp.GetResponseStream() 
        use reader = new IO.StreamReader(stream) 
        let html = reader.ReadToEnd()
        printfn "finished downloading %s" url 
        }

// a list of sites to fetch
let sites = ["http://www.bing.com";
             "http://www.google.com";
             "http://www.microsoft.com";
             "http://www.amazon.com";
             "http://www.yahoo.com"]

#time                      // turn interactive timer on
sites 
|> List.map fetchUrlAsync  // make a list of async tasks
|> Async.Parallel          // set up the tasks to run in parallel
|> Async.RunSynchronously  // start them off
#time                      // turn timer off


// message queues
let printerAgent = MailboxProcessor.Start(fun inbox-> 

    // the message processing function
    let rec messageLoop() = async{
        
        // read a message
        let! msg = inbox.Receive()
        
        // process a message
        printfn "message is: %s" msg

        // loop to top
        return! messageLoop()  
        }

    // start the loop 
    messageLoop() 
    )

printerAgent.Post "hello" 
printerAgent.Post "hello again" 
printerAgent.Post "hello a third time" 
























// ================== Completeness =================
// impure code when needed
let mutable counter = 0

// create C# compatible classes and interfaces
type IEnumerator<'a> = 
    abstract member Current : 'a
    abstract MoveNext : unit -> bool 

// extension methods
type System.Int32 with
    member this.IsEven = this % 2 = 0

let i=20
if i.IsEven then printfn "'%i' is even" i

// UI code
open System.Windows.Forms 
let form = new Form(Width= 400, Height = 300, Visible = true, Text = "Hello World") 
form.TopMost <- true
form.Click.Add (fun args-> printfn "clicked!")
form.Show()
