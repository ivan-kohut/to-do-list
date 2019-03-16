(function (app) {

  var usersURL = "/api/v1/users";

  app.initTwoFactorAuthenticationLogic = function () {

    callAPI(`${usersURL}/account/two-factor-authentication-enabled`, "GET", null, function (isTwoFactorAuthenticationEnabled) {

      if (isTwoFactorAuthenticationEnabled === true) {
        $("#enable").hide();
      } else {
        $("#disable").hide();
      }

    });
  };

  app.initQrCode = function () {

    $("#enable-link").hide();
    $("#authentication-code").show();

    callAPI(`${usersURL}/account/authenticator-uri`, "GET", null, function (authenticatorUri) {
      generateQrCode(authenticatorUri);
    });
  };

  app.enableTwoFactorAuthentication = function () {

    var data = {
      code: $("#two-factor-token").val()
    };

    callAPI(`${usersURL}/account/enable-two-factor-authentication`, "PUT", data, function () {

      window.location.href = "/";

    }, function (error) {
      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["Code", "errors"].forEach(function (s) {
        if (responseJson[s] !== undefined) {
          showErrors(responseJson[s]);
        }
      });
    });
  };

  app.disableTwoFactorAuthentication = function () {
    callAPI(`${usersURL}/account/disable-two-factor-authentication`, "PUT", null, function () {
      window.location.href = "/";
    });
  };

  app.changePassword = function () {

    var data = {
      oldPassword: $("#old-password").val(),
      newPassword: $("#new-password").val(),
      confirmNewPassword: $("#confirm-new-password").val()
    };

    callAPI(`${usersURL}/account/change-password`, "POST", data, function () {

      window.location.href = "/";

    }, function (error) {

      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["OldPassword", "NewPassword", "ConfirmNewPassword", "errors"].forEach(function (s) {
        if (responseJson[s] !== undefined) {
          showErrors(responseJson[s]);
        }
      });
    });
  };

  function generateQrCode(authenticatorUri) {
    new QRCode($("#qrCode")[0],
      {
        text: authenticatorUri,
        width: 150,
        height: 150
      });
  }

  function callAPI(url, method, data, callback, errorCallback) {
    $.ajax({
      beforeSend: function (request) {
        request.setRequestHeader("Authorization", `Bearer ${localStorage.getItem("auth-token")}`);
      },
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback,
      error: errorCallback
    });
  }

  function showErrors(errors) {

    var $errors = $("#errors");

    errors.forEach(function (error) {
      $errors.append(
        $("<li>").append(error)
      );
    });
  }

})(app = window.app || {});