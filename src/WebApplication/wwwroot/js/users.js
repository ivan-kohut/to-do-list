(function (app) {

  var usersURL = "/api/v1/users";
  var usersItems = {};

  app.init = function () {

    callAPI(usersURL, "GET", null, function (data) {
      for (var i = 0; i < data.length; i++) {
        $("#user-list").append(
          $("<li>")
            .attr("id", data[i].id)
            .append(`${data[i].name} - ${data[i].email} - ${data[i].isEmailConfirmed} - <a onclick='app.showOrHideItems(${data[i].id})'>Show or hide items</a> - <a onclick='app.deleteUser(${data[i].id})'>delete</a>`)
            .append($("<ul>"))
        );
      }
    });
  };

  app.showOrHideItems = function (id) {

    var items = usersItems[id];

    if (items === undefined) {
      callAPI(`${usersURL}/${id}/items`, "GET", null, function (data) {
        usersItems[id] = data;
        showOrHideItemsInList(id, data);
      });
    } else {
      showOrHideItemsInList(id, items);
    }
  };

  app.deleteUser = function (id) {

    callAPI(`${usersURL}/${id}`, "DELETE", null, function () {
      $(`#${id}`).remove();
    });
  };

  function showOrHideItemsInList(id, data) {

    var $items = $(`#${id}`).find("ul");

    if ($items.find("li").length === 0) {
      for (var i = 0; i < data.length; i++) {
        $items.append(
          $("<li>").append(data[i].text)
        );
      }
    } else {
      $items.find("li").remove();
    }
  }

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
