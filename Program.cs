using Newtonsoft.Json.Linq;
using RestSharp;
using System;
//using Newtonsoft.Json;


namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var token = Authenticate();

            String worker = "5637145201";
            String date = "2019-09-05";
            String hours = "4";
            String project = "00000101";
            String activity = "W00002480";
            String dataAreaId = "ussi";

            var timesheetNbr = WriteEntry(token, worker, date);
            WriteDetails(token, worker, date, timesheetNbr, project, activity, hours, dataAreaId);

            date = "2019-09-06";
            hours = "5";
            activity = "W00002388";

            timesheetNbr = WriteEntry(token, worker, date);
            WriteDetails(token, worker, date, timesheetNbr, project, activity, hours, dataAreaId);

            Console.ReadLine();
        }

        private static string Authenticate()
        {
            String id = "abcd3f06-f260-48f7-adad-6ae62a81374f";
            String secret = "SOqbCvR5skVbS23fips5iwd14aANiM8uPQbJZGWIqAA=";
            String resource = "00000015-0000-0000-c000-000000000000";


            // authenticate

            var client = new RestClient("https://login.windows.net/adacta-group.com/oauth2/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=all&client_id=" + id + "&client_secret=" + secret + "&resource=" + resource, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic resp = JObject.Parse(response.Content);
            String token = resp.access_token;

            return token;
        }

        private static String WriteEntry(String token, String worker, string date)
        {

            var client = new RestClient("https://ad-ctp-10-38eb6867baef10230aos.cloudax.dynamics.com/api/services/TSTimesheetServices/TSTimesheetSubmissionService/findOrCreateTimesheet");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { _resource = worker, _timesheetDate = date });


            var response = client.Execute(request);
            dynamic resp = JObject.Parse(response.Content);

            Console.WriteLine($"StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}, Content: {response.Content}");

            var timesheetNbr = resp.parmTimesheetNbr;

            return timesheetNbr;
        }

        private static void WriteDetails(String token, String worker, string date, String timesheetNbr, String project, String activity, string hours, String dataAreaId)
        {
            //var entry = new JObject
            //{
            //    { "parmResource", worker },
            //    { "parmTimesheetNumber", headerId },
            //    { "parmProjectDataAreaId", dataAreaId },
            //    { "parmProjId", project },
            //    { "parmProjActivityNumber", activity },
            //    { "parmEntryDate", date },
            //    { "parmHrsPerDay", 4 },
            //    { "customFields", new JArray() },
            //};
            //var eList = new JArray();
            //eList.Add(entry);

            //var entryList = new JObject
            //{
            //    {"entryList", eList }
            //};


            //var tsEntryList = new JObject
            //{
            //    { "_tsTimesheetEntryList", entryList }
            //};

            var entry = new
            {
                parmResource = worker ,
                parmTimesheetNumber = timesheetNbr,
                parmProjectDataAreaId = dataAreaId ,
                parmProjId = project,
                parmProjActivityNumber = activity ,
                parmEntryDate = date ,
                parmHrsPerDay = 4
            };

            object[] eList = new object[1];
            eList[0] = entry;

            var entryList = new
            {
                entryList = eList
            };

            var tsEntryList = new
            {
                _tsTimesheetEntryList = entryList 
            };

            var client = new RestClient("https://ad-ctp-10-38eb6867baef10230aos.cloudax.dynamics.com/api/services/TSTimesheetServices/TSTimesheetSubmissionService/createOrUpdateTimesheetLine");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.RequestFormat = DataFormat.Json;
            request.AddBody(tsEntryList);

            IRestResponse response = client.Execute(request);
            dynamic resp = JObject.Parse(response.Content);

            Console.WriteLine($"StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}, Content: {response.Content}");

        }
    }
}
