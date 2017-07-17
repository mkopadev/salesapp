using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Tickets;

namespace SalesApp.Core.Api.Tickets
{
    public class TicketApi : ApiBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketApi"/> class.
        /// </summary>
        /// <param name="path">The relative path of the API</param>
        public TicketApi(string path) : base(path)
        {
        }

        public async Task<TicketSubmissionResponse> SubmitTicket(Ticket ticket)
        {
            Logger.Verbose("Submit Ticket.");

            string jsonString = JsonConvert.SerializeObject(ticket);
            Logger.Verbose(jsonString);
            ServerResponse<TicketSubmissionResponse> response = await PostJsonAsync<TicketSubmissionResponse>(jsonString);

            Logger.Verbose("API call done.");
            if (response == null)
            {
                Logger.Verbose("API NOT successfull");

                return new TicketSubmissionResponse()
                {
                    Success = false,
                    Text = null
                };
            }

            Logger.Verbose("API result: " + response.IsSuccessStatus);
            Logger.Verbose(response.RawResponse);

            // try getting the object from JSON
            TicketSubmissionResponse result = null;
            try
            {
                result = response.GetObject();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            // got a proper result, return it
            if (result != null)
            {
                result.Success = true;
                return result;
            }

            Logger.Verbose("Could not parse response.");

            // if exception was thrown, return the exception as text
            if (response.RequestException != null)
            {

                return new TicketSubmissionResponse()
                {
                    Success = false,
                    Text = response.RequestException.ToString()
                };
            }

            return new TicketSubmissionResponse()
            {
                Success = false,
                Text = "Unknown Error."
            };

        }
    }
}