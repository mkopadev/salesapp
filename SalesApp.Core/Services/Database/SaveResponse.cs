using SalesApp.Core.Api;

namespace SalesApp.Core.Services.Database
{
    public class SaveResponse<T>  
    {
        public T SavedModel { get; private set; }
        public PostResponse PostingResponse { get; private set; }

        public SaveResponse(T savedModel, PostResponse postingResponse)
        {
            this.PostingResponse = postingResponse;
            this.SavedModel = savedModel;
        }
    }
}