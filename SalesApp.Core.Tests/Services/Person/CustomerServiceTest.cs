using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Tests.Services.Person
{
    // [TestFixture] todo One or more tests in the fixture are broken
    public class CustomerServiceTest
    {
        private Guid guid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // [Test] Todo NSubstitute.Exceptions.ReceivedCallsException : Expected to receive a call matching: RegisterCustomer(Customer, Normal). Actually received no matching calls.
        public void RegisterCustomer_ApiSuccess()
        {
            Customer customer = new Customer
            {
                Id = guid,
                RequestId = guid,
                UserId = guid,
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"), Id = guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            var successApiCallResponse = Task.FromResult(new CustomerRegistrationResponse {Successful = true, RequestId = guid,  ResponseText = "Success" });

            var mockCustomerApi = Substitute.For<ICustomerApi>();
            mockCustomerApi.RegisterCustomer(customer, ApiTimeoutEnum.Normal).Returns(successApiCallResponse);

            var mockCustomerFallbackApi = Substitute.For<ICustomerApiFallback>();
            
            // create the object under test
           // CustomerService customerService = new CustomerService(mockCustomerApi, mockCustomerFallbackApi);

            var dataChannels = new List<DataChannel>();
            var numberOfTries = new List<int>();
            var registrationDones = new List<bool>();
            var responses = new List<CustomerRegistrationResponse>();
           /* customerService.RegistrationStatusChanged += (o, e) =>
            {
                dataChannels.Add(e.Channel);
                numberOfTries.Add(e.NumberOfTries);
                registrationDones.Add(e.Response.Successful);
                responses.Add(e.Response);
            };

            // run the actual methos under test
            customerService.RegisterCustomer(customer, true);*/
            mockCustomerApi.Received().RegisterCustomer(customer, ApiTimeoutEnum.Normal);
            mockCustomerFallbackApi.DidNotReceive().RegisterCustomer(customer);

            Assert.That(dataChannels.Count, Is.EqualTo(1));
            Assert.That(dataChannels[0], Is.EqualTo(DataChannel.Full));
            
            Assert.That(numberOfTries[0], Is.EqualTo(1));
            
            Assert.That(registrationDones[0], Is.EqualTo(true));
            
           Assert.That(responses[0], Is.EqualTo(successApiCallResponse.Result));

        }

        // [Test] Todo Test throws Null pointer exception, fix it
        public void RegisterCustomer_ApiSecondSuccess()
        {
            Customer customer = new Customer
            {
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"), Id = guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            var failedApiCallResponse = Task.FromResult(new CustomerRegistrationResponse { Successful = false, RequestId = Guid.Empty,  ResponseText = null });
            var successApiCallResponse = Task.FromResult(new CustomerRegistrationResponse { Successful = true, RequestId = guid, ResponseText = "Success" });
            successApiCallResponse.Result.Customer.Id = guid;
            var mockCustomerApi = Substitute.For<ICustomerApi>();
            mockCustomerApi.RegisterCustomer(customer, ApiTimeoutEnum.Normal).Returns(failedApiCallResponse, successApiCallResponse);

            var mockCustomerFallbackApi = Substitute.For<ICustomerApiFallback>();
            
            // create the object under test
            //CustomerService customerService = new CustomerService(mockCustomerApi, mockCustomerFallbackApi);

            var dataChannels = new List<DataChannel>();
            var numberOfTries = new List<int>();
            var registrationDones = new List<bool>();
            var responses = new List<CustomerRegistrationResponse>();
            /*customerService.RegistrationStatusChanged += (o, e) =>
            {
                dataChannels.Add(e.Channel);
                numberOfTries.Add(e.NumberOfTries);
                registrationDones.Add(e.Response.Successful);
                responses.Add(e.Response);
            };

            // run the actual methos under test
            customerService.RegisterCustomer(customer, true);*/
            mockCustomerApi.Received().RegisterCustomer(customer, ApiTimeoutEnum.Normal);
            mockCustomerFallbackApi.DidNotReceive().RegisterCustomer(customer);

            Assert.That(dataChannels.Count, Is.EqualTo(2));
            Assert.That(dataChannels[0], Is.EqualTo(DataChannel.Full));
            Assert.That(dataChannels[1], Is.EqualTo(DataChannel.Full));
            
            Assert.That(numberOfTries[0], Is.EqualTo(1));
            Assert.That(numberOfTries[1], Is.EqualTo(2));
            
            Assert.That(registrationDones[0], Is.EqualTo(false));
            Assert.That(registrationDones[1], Is.EqualTo(true));

            Assert.That(responses[0], Is.EqualTo(failedApiCallResponse.Result));
            Assert.That(responses[1], Is.EqualTo(successApiCallResponse.Result));

        }

        // [Test] Todo Test throws Null pointer exception, fix it
        public void RegisterCustomer_ApiFailSmsSuccess()
        {
            Customer customer = new Customer
            {
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"), Id = guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            var failedApiCallResponse = Task.FromResult(new CustomerRegistrationResponse { Successful = false, RequestId = Guid.Empty,  ResponseText = null });
            failedApiCallResponse.Result.Customer.Id = default(Guid);
            var successFallbackResponse = Task.FromResult(true);

            var mockCustomerApi = Substitute.For<ICustomerApi>();
            mockCustomerApi.RegisterCustomer(customer, ApiTimeoutEnum.Normal).Returns(failedApiCallResponse);

            var mockCustomerFallbackApi = Substitute.For<ICustomerApiFallback>();
            mockCustomerFallbackApi.RegisterCustomer(customer).Returns(successFallbackResponse);

            // create the object under test
            //CustomerService customerService = new CustomerService(mockCustomerApi, mockCustomerFallbackApi);

            var dataChannels = new List<DataChannel>();
            var numberOfTries = new List<int>();
            var registrationDones = new List<bool>();
            var responses = new List<CustomerRegistrationResponse>();
            /*customerService.RegistrationStatusChanged += (o, e) =>
            {
                dataChannels.Add(e.Channel);
                numberOfTries.Add(e.NumberOfTries);
                registrationDones.Add(e.Response.Successful);
                responses.Add(e.Response);
            };

            // run the actual methos under test
            customerService.RegisterCustomer(customer, true);*/
            mockCustomerApi.Received().RegisterCustomer(customer, ApiTimeoutEnum.Normal);
            mockCustomerFallbackApi.Received().RegisterCustomer(customer);

            Assert.That(dataChannels[0], Is.EqualTo(DataChannel.Full));
            Assert.That(dataChannels[1], Is.EqualTo(DataChannel.Full));
            Assert.That(dataChannels[2], Is.EqualTo(DataChannel.Full));
            Assert.That(dataChannels[3], Is.EqualTo(DataChannel.Fallback));

            Assert.That(numberOfTries[0], Is.EqualTo(1));
            Assert.That(numberOfTries[1], Is.EqualTo(2));
            Assert.That(numberOfTries[2], Is.EqualTo(3));
            Assert.That(numberOfTries[3], Is.EqualTo(-1));

            Assert.That(registrationDones[0], Is.EqualTo(false));
            Assert.That(registrationDones[1], Is.EqualTo(false));
            Assert.That(registrationDones[2], Is.EqualTo(false));
            Assert.That(registrationDones[3], Is.EqualTo(true));

        }

        // [Test] todo NSubstitute.Exceptions.ReceivedCallsException : Expected to receive a call matching: RegisterCustomer(Customer, Normal). Actually received no matching calls.
        public void RegisterCustomer_NoInternetSmsSuccess()
        {
            Customer customer = new Customer
            {
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"), Id = guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            var mockCustomerApi = Substitute.For<ICustomerApi>();
           
            var mockCustomerFallbackApi = Substitute.For<ICustomerApiFallback>();
            mockCustomerFallbackApi.RegisterCustomer(customer).Returns(Task.FromResult(true));

            // create the object under test
            //CustomerService customerService = new CustomerService(mockCustomerApi, mockCustomerFallbackApi);

            var dataChannels = new List<DataChannel>();
            var numberOfTries = new List<int>();
            var registrationDones = new List<bool>();
            var responses = new List<CustomerRegistrationResponse>();
           /* customerService.RegistrationStatusChanged += (o, e) =>
            {
                dataChannels.Add(e.Channel);
                numberOfTries.Add(e.NumberOfTries);
                registrationDones.Add(e.Response.Successful);
                responses.Add(e.Response);
            };

            // run the actual methos under test
            customerService.RegisterCustomer(customer, false);*/
            mockCustomerApi.DidNotReceive().RegisterCustomer(customer, ApiTimeoutEnum.Normal);
            mockCustomerFallbackApi.Received().RegisterCustomer(customer);

            Assert.That(dataChannels[0], Is.EqualTo(DataChannel.Fallback));
            Assert.That(registrationDones[0], Is.True);
            Assert.That(numberOfTries[0], Is.EqualTo(-1));
            Assert.That(responses[0], Is.Null);
            

        }

        // [Test] Todo NSubstitute.Exceptions.ReceivedCallsException : Expected to receive a call matching: RegisterCustomer(Customer, Normal). Actually received no matching calls.
        public void RegisterCustomer_NoInternetSmsFailed()
        {
            Customer customer = new Customer
            {
                Product = new Product { DisplayName = "DisplayName", ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"), Id = guid, SerialNumber = "serial" },
                FirstName = "John",
                LastName = "Doe",
                Phone = "01234456789",
                NationalId = "123123",
                DsrPhone = "0712345678"
            };

            var mockCustomerApi = Substitute.For<ICustomerApi>();
            
            var mockCustomerFallbackApi = Substitute.For<ICustomerApiFallback>();
            mockCustomerFallbackApi.RegisterCustomer(customer).Returns(Task.FromResult(false));

            // create the object under test
            //CustomerService customerService = new CustomerService(mockCustomerApi, mockCustomerFallbackApi);

            var dataChannels = new List<DataChannel>();
            var numberOfTries = new List<int>();
            var registrationDones = new List<bool>();
            var responses = new List<CustomerRegistrationResponse>();
           /* customerService.RegistrationStatusChanged += (o, e) =>
            {
                dataChannels.Add(e.Channel);
                numberOfTries.Add(e.NumberOfTries);
                registrationDones.Add(e.Response.Successful);
                responses.Add(e.Response);   
            };

            // run the actual methos under test
            customerService.RegisterCustomer(customer, false);*/
            mockCustomerApi.DidNotReceive().RegisterCustomer(customer, ApiTimeoutEnum.Normal);
            mockCustomerFallbackApi.Received().RegisterCustomer(customer);

            Assert.That(dataChannels[0], Is.EqualTo(DataChannel.Fallback));
            Assert.That(registrationDones[0], Is.False);
            Assert.That(numberOfTries[0], Is.EqualTo(-1));
            Assert.That(responses[0], Is.Null);
            
        }
    }
}