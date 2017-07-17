using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Chama;
using SalesApp.Core.BL.Controllers.Chama;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.Enums;
using SalesApp.Core.ViewModels.Chama;

namespace SalesApp.Core.Tests.ViewModels.Chama
{
    public class GroupSelectionViewModelTests : MvxTestBase
    {
        private GroupSelectionViewModel _viewModel;
        private string _json = "[{\"GroupId\":1,\"Title\":\"Group type\",\"Description\":\"Please select group type\",\"Options\":[{\"OptionId\":2,\"OptionName\":\"Chama\"},{\"OptionId\":3,\"OptionName\":\"MFI\"}]},{\"GroupId\":4,\"Title\":\"Registered by\",\"Description\":\"Please select where the group is registered\",\"ParentId\":2,\"Options\":[{\"OptionId\":5,\"OptionName\":\"Nairobi\"},{\"OptionId\":6,\"OptionName\":\"Mombasa\"},{\"OptionId\":7,\"OptionName\":\"Kisumu\"},{\"OptionId\":8,\"OptionName\":\"Machakos\"},{\"OptionId\":9,\"OptionName\":\"Thika\"},{\"OptionId\":10,\"OptionName\":\"Kakamega\"},{\"OptionId\":11,\"OptionName\":\"Embakasi\"},{\"OptionId\":12,\"OptionName\":\"Kitui\"},{\"OptionId\":13,\"OptionName\":\"Makueni\"},{\"OptionId\":14,\"OptionName\":\"Kiambu\"}]},{\"GroupId\":15,\"Title\":\"Group name\",\"Description\":\"Please select the name of the group\",\"ParentId\":5,\"Options\":[{\"OptionId\":16,\"OptionName\":\"Nairobi CBD\"},{\"OptionId\":17,\"OptionName\":\"Nairobi Eastlands\"},{\"OptionId\":18,\"OptionName\":\"Nairobi Embakasi\"},{\"OptionId\":19,\"OptionName\":\"Nairobi Eistghleigh\"},{\"OptionId\":20,\"OptionName\":\"Umoja Inner Core\"},{\"OptionId\":21,\"OptionName\":\"Nairobi Westlands\"},{\"OptionId\":22,\"OptionName\":\"Langata Area\"},{\"OptionId\":23,\"OptionName\":\"Karen & Environs\"},{\"OptionId\":24,\"OptionName\":\"Nairobi Zone 5\"},{\"OptionId\":25,\"OptionName\":\"Msa Road Branch\"}]},{\"GroupId\":26,\"Title\":\"Group name\",\"Description\":\"Please select the name of the group\",\"ParentId\":6,\"Options\":[{\"OptionId\":27,\"OptionName\":\"Mombasa Group 3\"},{\"OptionId\":28,\"OptionName\":\"Mombasa Group 4\"}]}]";
        private ChamaController _chamaController;
        private ChamaApi _chamaApi;
        private string _urlParams = string.Format("?serverTimestamp={0}", DateTime.Now);

        [SetUp]
        public void Initialize()
        {
            this._chamaController = Substitute.For<ChamaController>();
            this._chamaApi = Substitute.For<ChamaApi>(this._urlParams);

            this._viewModel = new GroupSelectionViewModel(this._chamaController, this._chamaApi)
            {
                AllGroups = JsonConvert.DeserializeObject<List<Group>>(this._json)
            };

            this._viewModel.DisplayedGroups.Add(this._viewModel.AllGroups[0]);
        }

        private void ClickAGroup(Group groupToClick)
        {
            this._viewModel.GroupItemClick.Execute(groupToClick);
        }

        private void ClickAnOption(Option optionToClick)
        {
            this._viewModel.OptionItemClick.Execute(optionToClick);
        }

        [Test]
        public void TestGroupClickDisplaysTheGroupsOptions()
        {
            this.ClearAll();

            Group groupToClick = this._viewModel.DisplayedGroups[0];
            this.ClickAGroup(groupToClick);

            Assert.That(this._viewModel.ClickedGroup, Is.EqualTo(groupToClick));
            Assert.True(this._viewModel.ShowSelectionUi);
        }

        [Test]
        public void TestOptionItemClick()
        {
            this.ClearAll();

            // Click a group
            Group groupToClick = this._viewModel.DisplayedGroups[0];
            this.ClickAGroup(groupToClick);

            // Select an option
            Option optionToClick = groupToClick.Options[0];
            this.ClickAnOption(optionToClick);

            Assert.That(this._viewModel.DisplayedGroups[0].SelectedOption, Is.EqualTo(optionToClick));
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(2));
            Assert.False(this._viewModel.ShowSelectionUi);
            Assert.That(this._viewModel.FilterText, Is.Empty);
        }

        [Test]
        public void TestClickOptionItemManyTimes()
        {
            this.ClearAll();

            // Click a group
            this.ClickAGroup(this._viewModel.DisplayedGroups[0]);
            Assert.True(this._viewModel.ShowSelectionUi);

            // Select an option
            Option optionToClick = this._viewModel.DisplayedGroups[0].Options[0];
            this.ClickAnOption(optionToClick);

            Assert.That(this._viewModel.DisplayedGroups[0].SelectedOption, Is.EqualTo(optionToClick));
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(2));
            Assert.False(this._viewModel.ShowSelectionUi);
            Assert.That(this._viewModel.FilterText, Is.Empty);

            // Click the same group
            this.ClickAGroup(this._viewModel.DisplayedGroups[0]);
            Assert.True(this._viewModel.ShowSelectionUi);

            // Select the same option
            this.ClickAnOption(optionToClick);

            // same assertions remain
            Assert.That(this._viewModel.DisplayedGroups[0].SelectedOption, Is.EqualTo(optionToClick));
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(2));
            Assert.False(this._viewModel.ShowSelectionUi);
            Assert.That(this._viewModel.FilterText, Is.Empty);
        }

        [Test]
        public void TestClickDifferentOtionOnSameGroup()
        {
            this.ClearAll();

            // Click a group
            this.ClickAGroup(this._viewModel.DisplayedGroups[0]);
            Assert.True(this._viewModel.ShowSelectionUi);

            // Select an option
            Option optionToClick = this._viewModel.DisplayedGroups[0].Options[0];
            this.ClickAnOption(optionToClick);

            Assert.That(this._viewModel.DisplayedGroups[0].SelectedOption, Is.EqualTo(optionToClick));
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(2));
            Assert.False(this._viewModel.ShowSelectionUi);
            Assert.That(this._viewModel.FilterText, Is.Empty);

            // Click the same group
            this.ClickAGroup(this._viewModel.DisplayedGroups[0]);
            Assert.True(this._viewModel.ShowSelectionUi);

            // Select the different option
            this.ClickAnOption(this._viewModel.DisplayedGroups[0].Options[1]);

            // same assertions remain
            Assert.That(this._viewModel.DisplayedGroups[0].SelectedOption, Is.EqualTo(this._viewModel.DisplayedGroups[0].Options[1]));
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.False(this._viewModel.ShowSelectionUi);
            Assert.That(this._viewModel.FilterText, Is.Empty);
        }

        [Test]
        public void TestClickAFinalOptionEnablesNextButton()
        {
            this.ClearAll();

            // Click a group [Group Types]
            Group groupToClick = this._viewModel.DisplayedGroups[0];
            this.ClickAGroup(groupToClick);

            // Select option [Chama]
            Option optionToClick = groupToClick.Options[0];
            this.ClickAnOption(optionToClick);

            // Click group [Group Locations]
            this.ClickAGroup(this._viewModel.DisplayedGroups[1]);

            // Now select an option
            this.ClickAnOption(this._viewModel.DisplayedGroups[1].Options[0]);

            // Click a group [Group names]
            this.ClickAGroup(this._viewModel.DisplayedGroups[2]);

            Assert.False(this._viewModel.EnableNextButton);

            // Now select a final option [Group Name]
            this.ClickAnOption(this._viewModel.DisplayedGroups[2].Options[0]);

            Assert.True(this._viewModel.EnableNextButton);
        }

        [Test]
        public void TestClearFilterSetsItToEmptyString()
        {
            this.ClearAll();

            this._viewModel.ClearFilter.Execute(null);

            Assert.True(string.IsNullOrEmpty(this._viewModel.FilterText));
        }

        [Test]
        public void TestGroupsLiveSearch()
        {
            this.ClearAll();

            // Click a group type
            Group groupToClick = this._viewModel.DisplayedGroups[0];
            this.ClickAGroup(groupToClick);

            // Select option [Chama]
            Option optionToClick = groupToClick.Options[0];
            this.ClickAnOption(optionToClick);

            // Click group [Chama]
            this.ClickAGroup(this._viewModel.DisplayedGroups[1]);

            // Now do a live search
            this._viewModel.FilterText = "Nairobi Group 1";

            // The following groups should be displayed; Nairobi Group 1, Nairobi Group 10
            Assert.That(this._viewModel.DisplayedGroups[0].Options.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestSelectionRecreationWithAFinalOption()
        {
            string json = "[{\"Name\":\"Chama\",\"Key\":\"Group type\",\"Value\":\"2\"},{\"Name\":\"Nairobi\",\"Key\":\"Registered by\",\"Value\":\"5\"},{\"Name\":\"Nairobi CBD\",\"Key\":\"Group name\",\"Value\":\"16\"}]";
            List<GroupKeyValue> selections = JsonConvert.DeserializeObject<List<GroupKeyValue>>(json);

            this._viewModel.RecreateSelections(selections);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(3));
            Assert.True(this._viewModel.EnableNextButton);
        }

        [Test]
        public void TestSelectionRecreationWithNoFinalOption()
        {
            string json = "[{\"Name\":\"Chama\",\"Key\":\"Group type\",\"Value\":\"2\"},{\"Name\":\"Nairobi\",\"Key\":\"Registered by\",\"Value\":\"5\"}]";
            List<GroupKeyValue> selections = JsonConvert.DeserializeObject<List<GroupKeyValue>>(json);

            this._viewModel.RecreateSelections(selections);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(2));
            Assert.False(this._viewModel.EnableNextButton);
        }

        [Test]
        public void TestSelectionRecreationWhenNoOptionWasClicked()
        {
            this._viewModel.RecreateSelections(this._viewModel.SelectedGroups);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.False(this._viewModel.EnableNextButton);
        }

        [Test]
        public async void TestUpdateGroupProperty()
        {
            this.ClearAll();

            string json = "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":4,\"Title\":\"Registered by\",\"Description\":\"Please select the SCM where the group is registered\",\"ParentId\":2,\"Status\":\"Modified\"}]}";

            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));

            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups[1].Description, Is.EqualTo("Please select the SCM where the group is registered"));
        }

        private async Task<ServerResponse<GroupInfo>> GetUpdate(string rawJson)
        {
            ServerResponse<GroupInfo> response = new ServerResponse<GroupInfo>
            {
                IsSuccessStatus = true,
                RawResponse = rawJson,
                Status = ServiceReturnStatus.Success,
                StatusCode = HttpStatusCode.OK
            };

            return await Task.Run(() => response);
        }

        [Test]
        public async void TestReplaceAnExistingGroup()
        {
            this.ClearAll();

            string json = "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":4,\"Title\":\"Registered by\",\"Description\":\"Please select the SCM where the group is registered\",\"ParentId\":2,\"Status\":\"Deleted\"},{\"GroupId\":29,\"Title\":\"Group name\",\"Description\":\"Please select the name of the chama\",\"ParentId\":2,\"Status\":\"New\",\"Options\":[{\"OptionId\":30,\"OptionName\":\"Mombasa Group 3\"},{\"OptionId\":31,\"OptionName\":\"Mombasa Group 4\"}]}]}";

            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));

            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups[3].Description, Is.EqualTo("Please select the name of the chama"));
        }

        [Test]
        public async void TestAddANewGroup()
        {
            this.ClearAll();

            string json =
                "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":29,\"Title\":\"Group name\",\"Description\":\"Please select the name of the chama\",\"ParentId\":3,\"Status\":\"New\",\"Options\":[{\"OptionId\":30,\"OptionName\":\"Mombasa Group 3\"},{\"OptionId\":31,\"OptionName\":\"Mombasa Group 4\"}]}]}";

            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));

            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups.Count, Is.EqualTo(5));
            Assert.That(this._viewModel.AllGroups[4].ParentId, Is.EqualTo(3));
        }

        [Test]
        public async void TestUpdateOptionProperty()
        {
            this.ClearAll();

            string json =
                "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":4,\"Title\":\"Registered By\",\"Description\":\"Please select where the group is registered\",\"ParentId\":2,\"Status\":\"Modified\",\"Options\":[{\"OptionId\":5,\"OptionName\":\"Nairobi City\",\"Status\":\"Modified\"}]}]}";
            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));
            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups[1].Options.Count, Is.EqualTo(10));
            Assert.That(this._viewModel.AllGroups[1].Options[0].OptionName, Is.EqualTo("Nairobi City"));
        }

        [Test]
        public async void TestRemoveAnOption()
        {
            this.ClearAll();

            string json =
                "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":4,\"Title\":\"Registered By\",\"Description\":\"Please select where the group is registered\",\"ParentId\":2,\"Status\":\"Modified\",\"Options\":[{\"OptionId\":5,\"OptionName\":\"Nairobi\",\"Status\":\"Deleted\"}]}]}";

            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));
            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups[1].Options.Count, Is.EqualTo(9));
        }

        [Test]
        public async void TestAddANewOption()
        {
            this.ClearAll();

            string json = "{\"ServerTimeStamp\":\"2016-07-21T11:26\",\"Package\":[{\"GroupId\":4,\"Title\":\"Registered By\",\"Description\":\"Please select where the group is registered\",\"ParentId\":2,\"Status\":\"Modified\",\"Options\":[{\"OptionId\":29,\"OptionName\":\"Eldoret\",\"Status\":\"New\"}]}]}";
            this._chamaApi.MakeGetCallAsync<GroupInfo>(this._urlParams).ReturnsForAnyArgs(async a => await this.GetUpdate(json));
            await this._viewModel.RefreshGroupsOnClick.ExecuteAsync();

            Assert.False(this._viewModel.EnableNextButton);
            Assert.That(this._viewModel.DisplayedGroups.Count, Is.EqualTo(1));
            Assert.That(this._viewModel.AllGroups[1].Options.Count, Is.EqualTo(11));
            Assert.That(this._viewModel.AllGroups[1].Options[10].OptionName, Is.EqualTo("Eldoret"));
        }
    }
}
