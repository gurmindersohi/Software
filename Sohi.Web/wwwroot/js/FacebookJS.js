
function LoginDialog() {

    var token;

    FB.login(function (response) {
        if (response.authResponse) {

            token = response.authResponse["accessToken"];

        } else {
            console.log('User cancelled login or did not fully authorize.');
        }
    });

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