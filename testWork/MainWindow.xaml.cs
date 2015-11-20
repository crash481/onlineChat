using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Shared;
using MongoDB.Driver.Builders;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Timers;





namespace testWork
{
 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const string connect = "mongodb://172.20.10.3"; // Здесь ввести адрес заппущеного сервера Nosql базы данных MongoDB
        static User userLogginedIn;

        static Timer timer = new Timer();

        static MongoClient client = new MongoClient(connect);
        static MongoServer server = client.GetServer();
        static MongoDatabase mongoDatabase = server.GetDatabase("forTestWork");
        
        static MongoCollection<Message> messageCollection = mongoDatabase.GetCollection<Message>("Messages");
        static MongoCollection<User> userCollection = mongoDatabase.GetCollection<User>("Users");
        static MongoCollection<User> onlineUserCollection = mongoDatabase.GetCollection<User>("OnlineUsers");

        public event PropertyChangedEventHandler PropertyChanged;

        private long messageCounter = 0;
        private long onlineUsersCounter = 0;

        private string _messageBoxContext;
        public string messageBoxContext
        {
            get
            {
                return _messageBoxContext;
            }
            set
            {
                if (value != _messageBoxContext)
                {
                    _messageBoxContext = value;
                    OnPropertyChanged("messageBoxContext");
                }
            }
        }

        private string _onlineUsersBoxContext;
        public string onlineUsersBoxContext
        {
            get
            {
                return _onlineUsersBoxContext;
            }
            set
            {
                if (value != _onlineUsersBoxContext)
                {
                    _onlineUsersBoxContext = value;
                    OnPropertyChanged("onlineUsersBoxContext");
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public MainWindow()
        {
            
            InitializeComponent();
            Application.Current.SessionEnding += onClose;
            this.Closing += onClose;

            
            timer.Interval = 300;
            timer.Elapsed += updateFromDatabase;
            timer.Enabled = true;
            

            messageBox.DataContext = this;
            onlineUsersBox.DataContext = this;

           
          
        }




        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Message  msg = new Message(userLogginedIn.name, messageToSend.Text, 
             System.DateTime.UtcNow.ToLocalTime().ToString());

            messageCollection.Insert(msg);
            messageToSend.Clear();
        }


        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

            IMongoQuery query = Query<User>.EQ(x => x.name, loginBox.Text);
            if (userCollection.Count(query) <= 0 )
            {
                MessageBox.Show("Пользователя с таким логином не существует", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            User userToLoggin = userCollection.FindOne(query);
            if (userToLoggin.password.Equals(passwordBox.Text))
            {
                try { onlineUserCollection.Insert(userToLoggin); }
                catch (MongoDuplicateKeyException ex)
                {
                    MessageBox.Show("Пользователь с данным логином уже в сети", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (userLogginedIn != null)
                {
                    IMongoQuery removeQuery = Query<User>.EQ(x => x.name, userLogginedIn.name);
                    onlineUserCollection.Remove(removeQuery);
                }
                userLogginedIn = userToLoggin;
            }
            else
            {
                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }


            messageToSend.IsEnabled = true;
            messageBox.IsEnabled = true;
            sendButton.IsEnabled = true;
            loginBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            loginButton.IsEnabled=false;
            registerButton.IsEnabled = false;
            
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            IMongoQuery query = Query<User>.EQ(x => x.name, loginBox.Text);
            if (userCollection.Count(query) == 0)
            {
                userCollection.Insert(new User(loginBox.Text, passwordBox.Text));
                loginButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void onClose(object sender, CancelEventArgs e)
        {
            if (userLogginedIn != null)
            {
                IMongoQuery query = Query<User>.EQ(x => x.name, userLogginedIn.name);
                onlineUserCollection.Remove(query);
            }
        }
        
        private void updateFromDatabase(object sender, ElapsedEventArgs e)
        {
            if (userLogginedIn == null) { return; }
            long messagesNow = messageCollection.Count();
            if (messagesNow > messageCounter)
            {   

                messageCounter = messagesNow;
                List<Message> previousMesages = messageCollection.FindAll().ToList();
                messageBoxContext = "";
                foreach (Message msg in previousMesages)
                {
                    messageBoxContext += Environment.NewLine + Environment.NewLine +"-   " + msg.userName +
                        Environment.NewLine + msg.time + ": "+ msg.message;
                }
            }

            long onlineUsersNow = onlineUserCollection.Count();
             if (onlineUsersNow > onlineUsersCounter)
            {
                onlineUsersCounter = onlineUsersNow;
                List<User> onlineUsers = onlineUserCollection.FindAll().ToList();
                onlineUsersBoxContext = "";
                foreach (User user in onlineUsers)
                {
                    onlineUsersBoxContext += user.name + Environment.NewLine;
                }
             }
        }

        private void messageBox_TextChanged(object sender, TextChangedEventArgs e)
        {    
                messageBox.ScrollToEnd();
        }

    }

}
