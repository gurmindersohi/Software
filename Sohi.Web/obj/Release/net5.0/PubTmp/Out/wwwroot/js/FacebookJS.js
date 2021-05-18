
//async function LoginDialog() {

//    var token;

//    FB.login(function (response) {
//        if (response.authResponse) {

//            token = response.authResponse["accessToken"];

//        } else {
//            console.log('User cancelled login or did not fully authorize.');
//        }
//    });
//        //, { scope: 'public_profile,instagram_basic,instagram_content_publish,instagram_manage_comments,instagram_manage_insights,read_insights,pages_manage_cta,pages_manage_instant_articles,pages_manage_engagement,pages_manage_metadata,pages_manage_posts,pages_messaging,pages_read_engagement,pages_read_user_content,pages_show_list' });

//    return token;
//}



function LoginDialog() {

    var token;

    FB.login(function (response) {
        if (response.authResponse) {

            token = response.authResponse["accessToken"];

        } else {
            console.log('User cancelled login or did not fully authorize.');
            
        }
    }, { scope: 'email,public_profile' });

            //scope: 'email,public_profile,instagram_basic, instagram_content_publish, instagram_manage_comments, instagram_manage_insights, read_insights, pages_manage_cta, pages_manage_instant_articles, pages_manage_engagement, pages_manage_metadata, pages_manage_posts, pages_messaging, pages_read_engagement, pages_read_user_content, pages_show_list' 

        

    return token;
}




//async function PagesLoginDialog() {

//    var result;

//    FB.login(async function (response) {
//        if (response.authResponse) {

//            FB.api('me/accounts?fields=id,name,picture,access_token', function (response) {
//                result = response.data;
//                //console.log('Good to see you, ' + response.data[0].name + '.');
//            });

//        } else {
//            console.log('User cancelled login or did not fully authorize.');
//        }
//    });

//    return result;
//}



//function LoginDialog() {
//    FB.login(function (response) {
//        if (response.authResponse) {
//            console.log('Welcome!  Fetching your information.... ');
//            FB.api('/me', function (response) {
//                console.log('Good to see you, ' + response.name + '.');
//                return response;
//            });
//        } else {
//            console.log('User cancelled login or did not fully authorize.');
//        }
//        return response;
//    });


//}