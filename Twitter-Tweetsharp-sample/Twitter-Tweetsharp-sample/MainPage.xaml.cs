using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using TweetSharp;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;

namespace Twitter_Tweetsharp_sample
{
    //How to proceed
    //get Request Token
    //get Authorization URI
    //get PIN code 
    //get Access token using PIN code and Request Token
    //Finally tweet using the authorized Access Tokens 

        public partial class MainPage : PhoneApplicationPage
    {

        private OAuthRequestToken requestToken;
        private TwitterService service;
        Dispatcher dispatchme = Deployment.Current.Dispatcher;



        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void requesttokenbtn_Click(object sender, RoutedEventArgs e)
        {
              //create a new service and enter your consumerkey and consumersecret key
              //consumer key and secret key can be found inside your application at dev.twitter.com
              service = new TwitterService(TwitterKeys.ConsumerKey, TwitterKeys.ConsumerKeySecret);
              //initialization of of one of many callbacks with OAuthRequestToken
              var callback = new Action<OAuthRequestToken, TwitterResponse>(CallBackToken);
              //oob refers to PIN based authorization process
              service.GetRequestToken("oob", CallBackToken);
              
        }


        void CallBackToken(OAuthRequestToken rtoken, TwitterResponse response)
        {
            requestToken = rtoken;
            //fetch authorization URI and store in a URI object
            Uri uri = service.GetAuthorizationUri(requestToken);
            //now that we have the URI, we will use Web Task to navigate to the URI
            WebBrowserTask wtask = new WebBrowserTask();
            wtask.Uri = uri;
            //This will lead the user to application authentication page. Once user authenticates acccount 
            //ping authorization process will show up
            dispatchme.BeginInvoke(() => wtask.Show());

            //Instead of using Web Task you can also use the embedded browser control as shown below
            //BrowserControl.Dispatcher.BeginInvoke(() => BrowserControl.Navigate(uri));

        }

        private void accesstokenbtn_Click(object sender, RoutedEventArgs e)
        {
            //check if PIN has been entered or not by the user
            if (String.IsNullOrEmpty(textbox1.Text))
                MessageBox.Show("Please enter PIN");
            else
            {
                try
                {
                    
                    var callmeback = new Action<OAuthAccessToken, TwitterResponse>(CallBackVerifiedResponse);
                    //most important part: now that we have both requestToken and PIN, as stated originally
                    //we will now fetch AccessToken and AccessTokenSecret by using the PIN and request token
                    service.GetAccessToken(requestToken, textbox1.Text, CallBackVerifiedResponse);
                 

                }
                catch
                {
                    //error
                }
            }
        }


         void CallBackVerifiedResponse(OAuthAccessToken atoken, TwitterResponse response)
        {
            
            
         if (atoken != null)
            {
                //authorization complete - access tokens are now available
                dispatchme.BeginInvoke(() => MessageBox.Show("Authentication successfull!"));
                //store the AccessToken and AccessTokenSecret in local static variables
                TwitterVariables.AccessToken = atoken.Token;
                TwitterVariables.AccessTokenSecret = atoken.TokenSecret;
                //to proceed, first we must authenticate with AccessToken and AccessTokenSecret
                service.AuthenticateWith(TwitterVariables.AccessToken, TwitterVariables.AccessTokenSecret);
                //Now we are ready to post a tweet

                //Important: 
                //store the access token and Access Token Secret permanently in Isolated storage 
                //so that user can avoid authenticating everytime he opens your application

            }

           

          
        }

         private void tweetbtn_Click(object sender, RoutedEventArgs e)
         {
             //Post a tweet 
             service.SendTweet(new SendTweetOptions { Status = "testing a tweetsharp WP sample. It works @weemundo! " }, (TwitterStatus status, TwitterResponse response) =>
             {
                 if (response.StatusCode == HttpStatusCode.OK)
                 {  
                     //mission accomplished
                     dispatchme.BeginInvoke(()=> MessageBox.Show("tweet posted!"));
                 }

                 else
                 {
                     throw new Exception(response.StatusCode.ToString());
                 }
             }
             );
         }


    }
}
