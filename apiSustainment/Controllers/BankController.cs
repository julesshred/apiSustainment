using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
//using System.Web.Mvc;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Http.Cors;

namespace apiSustainment.Controllers
{
    public static class AccountPersistentStatic
    {
        public static string data="";
        public static string balance = "";
    }
        public class AccountMovement
    {
        public string TranDateTime { get; set; }
        public string AccountNumber { get; set; }
        public string OperationType { get; set; }//Debit Credit
        public double Amount { get; set; } //
    }
    public class AccountOperation
    {
        public string Account { get; set; }
        public string OperationType { get; set; }//Debit Credit
        public double Amount { get; set; } //
    }
    public class BankController : ApiController
    {
        //object Session = HttpContext.Current.Session;
        private void CheckSessionAccountMovements()
        {
            var serializer = new JavaScriptSerializer();
            //AccountPersistentStaic.data += "init";
            if (AccountPersistentStatic.data.Length.Equals(0))
            {//set default movements //create new array of AccountMovements
                AccountMovement[] warrAccountMovements = { new AccountMovement { TranDateTime = System.DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), AccountNumber = "0010090", OperationType = "Debit", Amount = 100 },
                new AccountMovement { TranDateTime = System.DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), AccountNumber = "0010090", OperationType = "Debit", Amount = 100 },
                new AccountMovement { TranDateTime = System.DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), AccountNumber = "0010090", OperationType = "Debit", Amount = 100 } };
                //HttpContext.Session.SetString("AccountMovements", afd.ToString() );
                //obtain JSON text from AccountMovements Array
                var serializedResult = serializer.Serialize(warrAccountMovements);
                //var jsonString = JsonSerializer.Serialize<AccountMovement[]>(warrAccountMovements);
                AccountPersistentStatic.balance = Convert.ToString(500);//init with $500
                AccountPersistentStatic.data = serializedResult;
            }
        }
        // GET api/<controller>
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet()]
        public AccountMovement[] Get()
        {//serializing JSON in .Net: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-5-0
            //get session value of account movements

            var serializer = new JavaScriptSerializer();
            CheckSessionAccountMovements();
            
            string wSessionAccountMovements = Convert.ToString(AccountPersistentStatic.data) ;
            //get data from Session
            //setup session in .net core https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-5.0
            //obtain AccountMovements Array from JSON text (from the session value)
            AccountMovement[] warrAccountMovementsRet = serializer.Deserialize<AccountMovement[]>(wSessionAccountMovements);
            return warrAccountMovementsRet;// (AccountMovement[])sessionData;
            //return Ok();
        }


        // POST api/<controller>
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        //https://stackoverflow.com/questions/60904904/the-json-value-could-not-be-converted-to-system-string-when-attempting-to-call
        public IHttpActionResult Post(AccountOperation iNewAccountOperation )
        {
            try
            {
                var serializer = new JavaScriptSerializer( );
                CheckSessionAccountMovements();//initialize account movements
                //string wSessionAccountMovements = Convert.ToString( HttpContext.Current.Session["AccountMovements"] );//obtain values from session            
                string wSessionAccountMovements = AccountPersistentStatic.data ;
                //setup session in .net core https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-5.0
                //obtain AccountMovements Array from JSON text (from the session value)
                double wdblNewBalance = Convert.ToDouble(AccountPersistentStatic.balance) - iNewAccountOperation.Amount;
                if ( iNewAccountOperation.OperationType.Equals("Debit") && wdblNewBalance<0) return BadRequest("Insufficient funds");//account would be out of funds so raais an error
                // 400 Bad Request
                AccountMovement[] warrAccountMovementsRet = serializer.Deserialize<AccountMovement[]>(wSessionAccountMovements);
                //Load data onto Object from JSON string
                Array.Resize(ref warrAccountMovementsRet, warrAccountMovementsRet.Length + 1);//add item to array
                warrAccountMovementsRet[warrAccountMovementsRet.Length - 1] = new AccountMovement { TranDateTime = System.DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), AccountNumber = iNewAccountOperation.Account,
                        OperationType = iNewAccountOperation.OperationType, Amount = iNewAccountOperation.Amount };
                var jsonString = serializer.Serialize(warrAccountMovementsRet);//get JSON string from object
                AccountPersistentStatic.data = jsonString;
                AccountPersistentStatic.balance = wdblNewBalance.ToString();
                return Ok("OK");//200 OK
            }
            catch (Exception ex)
            {
                return BadRequest("Message:" + ex.Message);
            }
        }

    }
}