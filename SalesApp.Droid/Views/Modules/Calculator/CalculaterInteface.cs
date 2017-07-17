using System;
using System.Collections.Generic;
using Android.Content;
using Android.Webkit;
using Java.Interop;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.Calculator;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;
using Object = Java.Lang.Object;

namespace SalesApp.Droid.Views.Modules.Calculator
{
    public class CalculatorInteface : Object
    {
        Context _context;

        public CalculatorInteface(Context context)
        {
            this._context = context;
        }

        [Export]
        [JavascriptInterface]
        public string GetHash()
        {
            string hash = Resolver.Instance.Get<ISalesAppSession>().UserHash;
            return hash;
        }

        [Export]
        [JavascriptInterface]
        public string GetTrackingId()
        {
            string trackingId = Settings.Instance.GoogleAnalyticsTrackingId;
            return trackingId;
        }
        [Export]
        [JavascriptInterface]
        public string GetUrl()
        {
            string url = Resolver.Instance.Get<IConfigService>().ApiUrl;
            return url;
        }

        [Export]
        [JavascriptInterface]
        public string GetUserId()
        {
            Guid userId = Resolver.Instance.Get<ISalesAppSession>().UserId;
            return userId.ToString();
        }

        [Export]
        [JavascriptInterface]
        public void SaveProducts(string data)
        {
            List<SalesApp.Core.BL.Models.Calculator.Calculator> products = AsyncHelper.RunSync(async () => await new CalculatorController().GetAllAsync());
            if (products != null && products.Count > 0)
            {
                SalesApp.Core.BL.Models.Calculator.Calculator calc = new SalesApp.Core.BL.Models.Calculator.Calculator();
                if (products[0].Id != default(Guid))
                {
                    products[0].Products = data;
                    new CalculatorController().SaveAsync(products[0]);
                }
                else
                {
                    calc.Products = data;
                    new CalculatorController().SaveAsync(calc);
                }
            }
        }

        [Export]
        [JavascriptInterface]
        public string GetProducts()
        {
            string products = CalculatorFetchProducts.Instance.Product;
            if (string.IsNullOrWhiteSpace(products))
            {
                products = "{'Products':[{'ProductTypeId':'16732cb0-b418-e411-9439-000c29921969','Name':'Product 3','Deposit':2999.00,'CostPerDay':40.00,'TotalPrice':17599.00,'OptimalLoanDuration':365},{'ProductTypeId':'eaf05020-675c-e511-80c0-000d3a219a86','Name':'Product 4','Deposit':3500.00,'CostPerDay':50.00,'TotalPrice':21750.00,'OptimalLoanDuration':365},{'ProductTypeId':'c87f42e3-8ca8-e511-80c3-000d3a219a86','Name':'Product 400','Deposit':7999.00,'CostPerDay':125.00,'TotalPrice':53624.00,'OptimalLoanDuration':365},{'ProductTypeId':'91e97523-8dba-e511-80c8-000d3a22f4e7','Name':'Product 401','Deposit':3500.00,'CostPerDay':50.00,'TotalPrice':40000.00,'OptimalLoanDuration':730}]}";
            }

            return products;
        }
    }
}