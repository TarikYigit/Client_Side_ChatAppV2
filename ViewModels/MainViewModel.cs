using System;
using System.IO;
using System.Linq;
using ClientSideChatApp.Core;

namespace ClientSideChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private TcpChatService _masterChatService;

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string MyUsername { get; set; }

        public byte MyUserId { get; set; }

        public MainViewModel()
        {

            _masterChatService = new TcpChatService();

            _masterChatService.MessageReceived += BackgroundMessageSaver;

            _masterChatService.GroupMessageReceived += BackgroundGroupMessageSaver;

            CurrentView = new LoginViewModel(this, _masterChatService);

        }

        private void BackgroundGroupMessageSaver(byte groupId, byte senderId, int messageId, string senderName, string messageContent, string timeString)
        {

            if (CurrentView is GroupChatViewModel groupVM && groupVM.TargetGroup?.GroupId == groupId)
            {

                return; 

            }

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string folderPath = System.IO.Path.Combine(appData, "ClientSideChatApp", $"ChatLogs_{MyUsername}");

            System.IO.Directory.CreateDirectory(folderPath);

            string chatFilePath = System.IO.Path.Combine(folderPath, $"GroupChat_{groupId}.txt");

            string fileLine = $"{senderName}|{timeString}|{messageId}|{messageContent}|False\n";

            System.IO.File.AppendAllText(chatFilePath, fileLine);

            ClientSideChatApp.Helpers.WindowFlashHelper.FlashTaskbar();

            var group = _masterChatService.AllGroups.FirstOrDefault(g => g.GroupId == groupId);

            bool isLookingAtThisGroup = CurrentView is GroupChatViewModel groupVm && groupVm.TargetGroup.GroupId == groupId;

            if (group != null && !isLookingAtThisGroup)
            {
                group.UnreadCount++;
            }

            System.Media.SystemSounds.Asterisk.Play();

        }

        private void BackgroundMessageSaver(byte senderId, int messageId, string messageContent, string timeString)
        {
            if (CurrentView is ChatViewModel chatVM && chatVM.TargetUser?.UserId == senderId)
            {

                return;

            }

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string folderPath = System.IO.Path.Combine(appData, "ClientSideChatApp", $"ChatLogs_{MyUsername}");

            System.IO.Directory.CreateDirectory(folderPath);

            string chatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{senderId}.txt");

            var sender = _masterChatService.AllUsers.FirstOrDefault(u => u.UserId == senderId);

            string senderName = sender != null ? sender.Username : $"User_{senderId}";

            string fileLine = $"{senderName}|{timeString}|{messageId}|{messageContent}|False\n";

            System.IO.File.AppendAllText(chatFilePath, fileLine);

            ClientSideChatApp.Helpers.WindowFlashHelper.FlashTaskbar();

            var user = _masterChatService.AllUsers.FirstOrDefault(u => u.UserId == senderId);

            bool isLookingAtThisUser = CurrentView is ChatViewModel chatVm && chatVm.TargetUser.UserId == senderId;

            if (user != null && !isLookingAtThisUser)
            {

                user.UnreadCount++;

            }
            System.Media.SystemSounds.Asterisk.Play();

        }
    }
}