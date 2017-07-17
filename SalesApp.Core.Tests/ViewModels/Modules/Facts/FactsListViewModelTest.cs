using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Modules.Facts;
using SalesApp.Core.ViewModels.Modules.Facts;

namespace SalesApp.Core.Tests.ViewModels.Modules.Facts
{
    [TestFixture]
    public class FactsListViewModelTest : TestsBase
    {
        private FactsListViewModel _model;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            string factsJson = "[{\"Id\":\"7b0d1eb8-5120-4914-8dd0-d566a8c10749\",\"Title\":\"Fact 1 title\",\"Description\":\"Fact one description\"},{\"Id\":\"9c11f6e2-7081-4752-a89f-27f7c27d7ba3\",\"Title\":\"Fact 2 title\",\"Description\":\"Fact 2 description\"},{\"Id\":\"5a4c3984-a0d4-4eef-bd29-b2bb0831a6a5\",\"Title\":\"Fact 3 title\",\"Description\":\"Fact three description\"},{\"Id\":\"3396d878-5bd0-4ddc-8491-8e9a85dfe62f\",\"Title\":\"Fact 4 title\",\"Description\":\"Fact 4 description\"},{\"Id\":\"e6ef7bc9-564c-41a1-a51c-c65c1c5c3649\",\"Title\":\"Fact 5 title\",\"Description\":\"Fact 5 description\"},{\"Id\":\"785cd38e-376f-4dce-a37a-d8922ee2ab7d\",\"Title\":\"Fact 6 title\",\"Description\":\"Fact 6 description\"},{\"Id\":\"1e8727ed-70f5-4b7f-b57c-9804a2de8330\",\"Title\":\"Fact 7 title\",\"Description\":\"Fact 7 description\"},{\"Id\":\"fb3b126d-0433-43b3-8ba8-9e222f0d6530\",\"Title\":\"Fact 8 title\",\"Description\":\"Fact 8 description\"}]";

            var assets = Substitute.For<IAssets>();
            assets.GetAssetAsString("Facts/facts.json").Returns(factsJson);

            this._model = new FactsListViewModel(assets)
            {
                LoadFactDetails = fact => { }
            };

            Assert.That(9, Is.EqualTo(this._model.Facts.Count));
        }

        [Test]
        public void TestClickingAFactOpensFactDetailsScreen()
        {
            Fact fact = this._model.Facts[0];
            this._model.LoadFactDetails(fact);
        }
    }
}