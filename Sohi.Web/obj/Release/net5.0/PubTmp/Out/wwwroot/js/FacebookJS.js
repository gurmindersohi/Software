
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