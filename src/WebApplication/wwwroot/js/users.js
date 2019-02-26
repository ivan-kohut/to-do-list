(function (app) {

  var usersURL = "/api/v1/users";

  app.init = function () {

    callAPI(usersURL, "GET", null, function (data) {
      for (var i = 0; i < data.length; i++) {
        $("#user-list").append(
          $("<li>")
            .attr("id", data[i].id)
            .append(`${data[i].name} - ${data[i].email} - ${data[i].isEmailConfirmed} - <a onclick='app.deleteUser(${data[i].id})'>delete</a>`)
        );
      }
    });
  };

  app.deleteUser = function (id) {

    callAPI(`${usersURL}/${id}`, "DELETE", null, function () {
      $(`#${id}`).remove();
    });
  };

  function callAPI(url, method, data, callback) {
    $.ajax({
      beforeSend: function (request) {
        request.setRequestHeader("Authorization", `Bearer ${localStorage.getItem("auth-token")}`);
      },
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback
    });
  }

})(app = window.app || {});
