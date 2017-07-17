using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Chama;
using SalesApp.Core.BL.Controllers.Chama;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Chama;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Chama
{
    public class GroupSelectionViewModel : BaseViewModel
    {
        private Group _clickedGroup;
        private ObservableCollection<Group> _displayedGroups;
        private string _filterText;
        private MvxCommand<Group> _groupItemClick;
        private MvxCommand<Option> _optionItemClick;
        private MvxAsyncCommand _refreshGroupsOnClick;
        private MvxCommand _clearFilter;
        private bool _showSelectionUi;
        private bool _enableNextButton;
        private IDeviceResource _deviceResource = Resolver.Instance.Get<IDeviceResource>();
        private string _progressDialogMessage;
        private string _successMessage;
        private bool _isBusy;
        private ChamaController _chamaController;
        private ChamaApi _chamaApi;
        private List<Option> _fullList;
        private DateTime _serverTimeStamp;
        private bool _noSearchResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSelectionViewModel"/> class.
        /// Note: This constructor is called by JsonConver.Deserialize. Do not remove it.
        /// </summary>
        public GroupSelectionViewModel()
        {
            this._chamaApi = new ChamaApi("v1/chama/groups");
            this._chamaController = new ChamaController();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSelectionViewModel"/> class.
        /// </summary>
        /// <param name="controller">The database access controller</param>
        /// <param name="api">The api for fetching groups</param>
        public GroupSelectionViewModel(ChamaController controller, ChamaApi api)
        {
            this._chamaController = controller;
            this._chamaApi = api;
        }

        public List<Group> AllGroups { get; set; }

        public List<GroupKeyValue> SelectedGroups
        {
            get
            {
                if (this.DisplayedGroups.Count == 1)
                {
                    // user has not yet selected any option
                    return new List<GroupKeyValue>();
                }

                List<GroupKeyValue> selectedGroups = new List<GroupKeyValue>();

                foreach (var group in this.DisplayedGroups)
                {
                    GroupKeyValue keyValue = new GroupKeyValue
                    {
                        Key = group.Title,
                        Value = group.SelectedOption != null ? group.SelectedOption.OptionId.ToString() : "0",
                        Name = group.SelectedOption != null ? group.SelectedOption.OptionName : string.Empty
                    };

                    selectedGroups.Add(keyValue);
                }

                return selectedGroups;
            }
        }

        public ObservableCollection<Group> DisplayedGroups
        {
            get
            {
                if (this._displayedGroups == null)
                {
                    this._displayedGroups = new ObservableCollection<Group>();
                }

                return this._displayedGroups;
            }

            set
            {
                this.SetProperty(ref this._displayedGroups, value, () => this.DisplayedGroups);
            }
        }

        public string FilterText
        {
            get
            {
                return this._filterText;
            }

            set
            {
                if (this.ClickedGroup == null)
                {
                    return;
                }

                this.ClickedGroup.Searching = true;

                if (string.IsNullOrEmpty(this._filterText))
                {
                    this._fullList = this.GetCopy(this.ClickedGroup.Options);
                }

                this.SetProperty(ref this._filterText, value, () => this.FilterText);

                if (string.IsNullOrEmpty(this.FilterText))
                {
                    this.ClickedGroup.Options = this.GetCopy(this._fullList);
                    this._fullList = null;
                }
                else
                {
                    this.ClickedGroup.Options = this._fullList.Where(item => item.OptionName.ToLower().Contains(this.FilterText.ToLower())).ToList();
                }

                this.NoSearchResults = this.ClickedGroup.Options == null || this.ClickedGroup.Options.Count == 0;

                this.RaisePropertyChanged(() => this.ClickedGroup);
            }
        }

        public Group ClickedGroup
        {
            get
            {
                return this._clickedGroup;
            }

            set
            {
                this.SetProperty(ref this._clickedGroup, value, () => this.ClickedGroup);
            }
        }

        public bool ShowSelectionUi
        {
            get
            {
                return this._showSelectionUi;
            }

            set
            {
                this.SetProperty(ref this._showSelectionUi, value, () => this.ShowSelectionUi);
            }
        }

        public bool NoSearchResults
        {
            get
            {
                return this._noSearchResults;
            }

            set
            {
                this.SetProperty(ref this._noSearchResults, value, () => this.NoSearchResults);
            }
        }

        public bool EnableNextButton
        {
            get
            {
                return this._enableNextButton;
            }

            set
            {
                this.SetProperty(ref this._enableNextButton, value, () => this.EnableNextButton);
            }
        }

        public string ProgressDialogMessage
        {
            get
            {
                return this._progressDialogMessage;
            }

            set
            {
                this.SetProperty(ref this._progressDialogMessage, value, () => this.ProgressDialogMessage);
            }
        }

        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                this.SetProperty(ref this._isBusy, value, () => this.IsBusy);
            }
        }

        public string SuccessMessage
        {
            get
            {
                return this._successMessage;
            }

            set
            {
                this.SetProperty(ref this._successMessage, value, () => this.SuccessMessage);
            }
        }

        private List<Option> GetCopy(List<Option> source)
        {
            Option[] holder = new Option[source.Count];
            source.CopyTo(0, holder, 0, holder.Length);
            return holder.ToList();
        }

        private Group FindChildGroup(Option option)
        {
            Group group = this.AllGroups.Find(x => x.ParentId == option.OptionId);
            return group;
        }

        private bool IsParent(Option option)
        {
            Group child = this.FindChildGroup(option);
            return child != null;
        }

        private Option FindOptionById(int optionId)
        {
            Option result = null;

            foreach (var group in this.AllGroups)
            {
                foreach (var option in group.Options)
                {
                    if (option.OptionId == optionId)
                    {
                        result = option;
                    }
                }
            }

            return result;
        }

        private Group FindParentGroup(int optionId)
        {
            Group result = null;

            foreach (var group in this.AllGroups)
            {
                foreach (var option in group.Options)
                {
                    if (option.OptionId != optionId)
                    {
                        continue;
                    }

                    result = group;
                    break;
                }
            }

            return result;
        }

        private void UpdateGroup(Group clickedGroup, Option clickedOption)
        {
            int position = -1;
            for (int index = 0; index < this.DisplayedGroups.Count; index++)
            {
                Group currentGroup = this.DisplayedGroups[index];

                if (!clickedGroup.Equals(currentGroup))
                {
                    continue;
                }

                position = index;
                break;
            }

            if (position < 0)
            {
                return;
            }

            Group group = this.DisplayedGroups[position];

            Group backupGroup = new Group
            {
                GroupId = group.GroupId,
                Description = group.Description,
                Options = group.Options,
                SelectedOption = clickedOption,
                Title = group.Title
            };

            this.DisplayedGroups.RemoveAt(position);
            this.DisplayedGroups.Insert(position, backupGroup);

            this.RaisePropertyChanged(() => this.DisplayedGroups);
        }

        private void AddChildGroup(Group childGroup, Option selectedOption)
        {
            if (childGroup == null)
            {
                this.EnableNextButton = true;
                this.RemoveAllChildren(this.ClickedGroup);
                return;
            }

            if (this.DisplayedGroups.Contains(childGroup))
            {
                if (this.ClickedGroup.SelectedOption == selectedOption)
                {
                    return;
                }
            }

            this.EnableNextButton = false;
            this.RemoveAllChildren(this.ClickedGroup);
            this.DisplayedGroups.Add(childGroup);
        }

        private async Task RefreshGroups()
        {
            this.ProgressDialogMessage = this._deviceResource.UpdatingListPleaseWait;
            string urlParams = string.Format("?serverTimestamp={0}", this._serverTimeStamp.ToString("s"));
            this.IsBusy = true;

            try
            {
                ServerResponse<GroupInfo> response = await this._chamaApi.MakeGetCallAsync<GroupInfo>(urlParams);

                if (response.IsSuccessStatus)
                {
                    await this.ApplyUpdate(response.GetObject());
                }
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task SaveChamas(GroupInfo newGroups)
        {
            List<Chamas> groupList = await this._chamaController.GetAllAsync();
            Chamas newChama = new Chamas { ServerTimeStamp = newGroups.ServerTimeStamp };

            if (groupList != null && groupList.Count > 0)
            {
                newChama = groupList[0];
                newChama.Modified = DateTime.Now;
            }
            else
            {
                newChama.Created = new DateTime();
                newChama.Modified = new DateTime();
            }

            newChama.Package = JsonConvert.SerializeObject(newGroups.Package);
            await this._chamaController.SaveAsync(newChama);

            this.SuccessMessage = "Group lists have been updated";
        }

        private void RemoveAllChildren(Group parent)
        {
            int parentPosition = -1;
            for (int i = 0; i < this.DisplayedGroups.Count; i++)
            {
                if (this.DisplayedGroups[i].GroupId != parent.GroupId)
                {
                    continue;
                }

                parentPosition = i;
                break;
            }

            if (parentPosition < 0)
            {
                return;
            }

            for (int j = 0; j < this.DisplayedGroups.Count; j++)
            {
                if (j <= parentPosition)
                {
                    continue;
                }

                this.DisplayedGroups.RemoveAt(j);
                j--;
            }
        }

        public void RecreateSelections(List<GroupKeyValue> selections)
        {
            if (selections.Count == 0)
            {
                return;
            }

            this.DisplayedGroups.Clear();
            foreach (var selection in selections)
            {
                int optionId = int.Parse(selection.Value);

                if (optionId == 0)
                {
                    return;
                }

                Option option = this.FindOptionById(optionId);
                Group parent = this.FindParentGroup(optionId);
                this.ClickedGroup = parent;

                if (!this.IsParent(option))
                {
                    this.EnableNextButton = true;
                }

                if (this.DisplayedGroups.Count == selections.Count)
                {
                    this.ClickedGroup.SelectedOption = option;
                    return;
                }

                this.DisplayedGroups.Add(parent);
                this.UpdateGroup(this.ClickedGroup, option);
                Group childGroup = this.FindChildGroup(option);
                this.AddChildGroup(childGroup, option);
            }
        }

        public async Task<string> GetGroups()
        {
            string json;
            List<Chamas> groupList = await this._chamaController.GetAllAsync();
            if (groupList != null && groupList.Count > 0)
            {
                this._serverTimeStamp = groupList[0].ServerTimeStamp;
                json = JsonConvert.SerializeObject(groupList[0]);
            }
            else
            {
                json = this._deviceResource.InitialGroupList;
                GroupInfo newGroups = JsonConvert.DeserializeObject<GroupInfo>(json);
                this._serverTimeStamp = newGroups.ServerTimeStamp;
                await this.SaveChamas(newGroups);
            }

            return json;
        }

        private async Task ApplyUpdate(GroupInfo groupInfo)
        {
            List<Group> update = groupInfo.Package;

            if (update == null || update.Count == 0)
            {
                return;
            }

            bool modified = false;

            foreach (var group in update)
            {
                GroupStatus status = group.Status;

                switch (status)
                {
                        case GroupStatus.Deleted:
                            modified = this.AllGroups.Remove(group);
                        break;
                        case GroupStatus.Modified:
                            this.UpdateGroup(group);
                            modified = true;
                        break;
                        case GroupStatus.New:
                            this.AllGroups.Add(group);
                            modified = true;
                        break;
                }
            }

            if (!modified)
            {
                return;
            }

            GroupInfo chamas = new GroupInfo { ServerTimeStamp = groupInfo.ServerTimeStamp, Package = this.AllGroups };
            await this.SaveChamas(chamas);

            // reset all selections to the starting point
            this.DisplayedGroups = new ObservableCollection<Group>(new List<Group> { this.AllGroups[0] });
            this.EnableNextButton = false;
        }

        public async void InitialLoad(Lead lead)
        {
            this.ProgressDialogMessage = this._deviceResource.UpdatingListPleaseWait;

            string json = await this.GetGroups();
            this.AllGroups = JsonConvert.DeserializeObject<GroupInfo>(json).Package;
            this.DisplayedGroups.Add(this.AllGroups[0]);

            if (lead.GroupInfo != null)
            {
                this.RecreateSelections(lead.GroupInfo);
            }
        }

        private void UpdateGroup(Group group)
        {
            int position = this.FindGroupPosition(group);

            if (position < 0)
            {
                return;
            }

            this.AllGroups[position].Title = group.Title;
            this.AllGroups[position].Description = group.Description;
            this.AllGroups[position].ParentId = group.ParentId;

            if (group.Options == null || group.Options.Count == 0)
            {
                return;
            }

            foreach (var option in group.Options)
            {
                switch (option.Status)
                {
                    case GroupStatus.Deleted:
                        this.AllGroups[position].Options.Remove(option);
                        break;
                    case GroupStatus.Modified:
                        int optionPosition = this.FindOptionPosition(this.AllGroups[position], option);

                        if (optionPosition < 0)
                        {
                            return;
                        }

                        this.AllGroups[position].Options[optionPosition] = option;
                        break;
                    case GroupStatus.New:
                        this.AllGroups[position].Options.Add(option);
                        break;
                }
            }
        }

        private int FindOptionPosition(Group parent, Option option)
        {
            int index = -1;

            for (int i = 0; i < parent.Options.Count; i++)
            {
                if (!parent.Options[i].Equals(option))
                {
                    continue;
                }

                index = i;
                break;
            }

            return index;
        }

        private int FindGroupPosition(Group group)
        {
            int index = -1;

            for (int i = 0; i < this.AllGroups.Count; i++)
            {
                if (!this.AllGroups[i].Equals(group))
                {
                    continue;
                }

                index = i;
                break;
            }

            return index;
        }

        [JsonIgnore]
        public MvxAsyncCommand RefreshGroupsOnClick
        {
            get
            {
                this._refreshGroupsOnClick = this._refreshGroupsOnClick ?? new MvxAsyncCommand(this.RefreshGroups);

                return this._refreshGroupsOnClick;
            }
        }

        [JsonIgnore]
        public ICommand ClearFilter
        {
            get
            {
                this._clearFilter = this._clearFilter ?? new MvxCommand(() =>
                {
                    this.FilterText = string.Empty;
                });

                return this._clearFilter;
            }
        }

        [JsonIgnore]
        public ICommand OptionItemClick
        {
            get
            {
                this._optionItemClick = this._optionItemClick ?? new MvxCommand<Option>(option =>
                {
                    this.UpdateGroup(this.ClickedGroup, option);

                    Group childGroup = this.FindChildGroup(option);
                    this.AddChildGroup(childGroup, option);

                    this.ShowSelectionUi = false;
                    this.FilterText = string.Empty;
                });

                return this._optionItemClick;
            }
        }

        [JsonIgnore]
        public ICommand GroupItemClick
        {
            get
            {
                this._groupItemClick = this._groupItemClick ?? new MvxCommand<Group>(group =>
                {
                    group.Searching = false;
                    this.ClickedGroup = group;
                    this.ShowSelectionUi = true;
                });

                return this._groupItemClick;
            }
        }
    }
}
