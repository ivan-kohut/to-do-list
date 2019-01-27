(function (app) {

  var itemsURL = "/api/v1/items";
  var itemsStatusesURL = "/api/v1/select-lists/item-statuses";

  var selectedId = -1; // not selected

  var $inputField;

  app.init = function () {

    $inputField = $('#text_input');

    if (getAuthToken() === null) {

      $("#auth").show();

    } else {

      $("#items").show();

      callAPI(itemsURL, "GET", null, function (data) {
        for (var i = 0; i < data.length; i++) {
          putItem(data[i]);
        }
      });

      callAPI(itemsStatusesURL, "GET", null, function (data) {
        buildSelectList(data)
          .attr("id", "statuses-select-list")
          .attr("disabled", "disabled")
          .insertAfter($("#buttons"));
      });
    }
  };

  app.addItem = function () {
    var itemText = $inputField.val();

    if (!itemText)
      return false;

    callAPI(itemsURL, "POST", { text: itemText }, function (data) {
      putItem(data);
      clearInputField();

      if (isSelectedItem())
        resetItemState();
    });
  };

  app.updateItemText = function () {
    var newTextValue = $inputField.val();

    if (!isSelectedItem() || !newTextValue)
      return false;

    var statusId = parseInt($("#statuses-select-list").val());
    var item = getSelectedItem();
    var data = [{ name: "Text", value: newTextValue }, { name: "StatusId", value: statusId }];

    callAPI(`${itemsURL}/${selectedId}`, "PATCH", data, function () {
      item.text(newTextValue);
      item.attr("data-status-id", statusId);
    });
  };

  app.deleteItem = function () {
    if (!isSelectedItem())
      return false;

    callAPI(`${itemsURL}/${selectedId}`, "DELETE", null, function () {
      var item = getSelectedItem();
      var nextItem = item.next();

      item.remove();

      if (nextItem.length === 0) {
        resetItemState();
        clearInputField();
      } else {
        selectedId = nextItem.attr('id');
        $inputField.val(getSelectedItem().text());
      }
    });
  };

  app.moveUpItem = function () {
    if (!isSelectedItem())
      return false;

    var item = getSelectedItem();
    var previousItem = item.prev();

    if (previousItem.length !== 0) {

      var selectedItemReplaceOperation = { name: "Priority", value: parseInt(previousItem.attr("data-priority")) };
      var previousItemReplaceOperation = { name: "Priority", value: parseInt(item.attr("data-priority")) };

      callAPI(`${itemsURL}/${selectedId}`, "PATCH", [selectedItemReplaceOperation], function () {
        item.attr("data-priority", selectedItemReplaceOperation.value);
        item.insertBefore(previousItem);
      });

      callAPI(`${itemsURL}/${previousItem.attr("id")}`, "PATCH", [previousItemReplaceOperation], function () {
        previousItem.attr("data-priority", previousItemReplaceOperation.value);
      });
    }
  };

  app.moveDownItem = function () {
    if (!isSelectedItem())
      return false;

    var item = getSelectedItem();
    var nextItem = item.next();

    if (nextItem.length !== 0) {

      var selectedItemReplaceOperation = { name: "Priority", value: parseInt(nextItem.attr("data-priority")) };
      var nextItemReplaceOperation = { name: "Priority", value: parseInt(item.attr("data-priority")) };

      callAPI(`${itemsURL}/${selectedId}`, "PATCH", [selectedItemReplaceOperation], function () {
        item.attr("data-priority", selectedItemReplaceOperation.value);
        item.insertAfter(nextItem);
      });

      callAPI(`${itemsURL}/${nextItem.attr("id")}`, "PATCH", [nextItemReplaceOperation], function () {
        nextItem.attr("data-priority", nextItemReplaceOperation.value);
      });
    }
  };

  app.onItemSelect = function (id) {

    var statusesSelectListSelector = "#statuses-select-list"; 

    if (!isSelectedItem()) {
      changeColor("red", "blue");
      enableOrDisable(statusesSelectListSelector, true);
    }

    selectedId = id;

    var item = getSelectedItem();

    $inputField.val(item.text());
    $(statusesSelectListSelector).val(item.attr("data-status-id"));
  };

  function putItem(dataRow) {
    $("#item-list").append(
      $("<li>")
        .attr("id", dataRow.id)
        .attr("data-status-id", dataRow.statusId)
        .attr("data-priority", dataRow.priority)
        .attr("onclick", "app.onItemSelect(" + dataRow.id + ")")
        .append(dataRow.text)
    );
  }

  function buildSelectList(selectListItems) {

    var $selectList = $("<select>");

    selectListItems.forEach(function (selectListItem) {
      $selectList.append(
        $("<option>")
          .attr("value", selectListItem.value)
          .append(selectListItem.text)
      );
    });

    return $selectList;
  }

  function changeColor(from, to) {
    ['#update_button', '#delete_button', '#up_button', '#down_button'].forEach(function (selector) {
      $(selector).removeClass(from);
      $(selector).addClass(to);
    });
  }

  function isSelectedItem() {
    return selectedId !== -1;
  }

  function resetItemState() {
    selectedId = -1;
    changeColor("blue", "red");
    enableOrDisable("#statuses-select-list", false);
  }

  function clearInputField() {
    $inputField.val("");
  }

  function getSelectedItem() {
    return $("#" + selectedId);
  }

  function enableOrDisable(selector, isEnabling) {

    var disabled = "disabled";
    var $selector = $(selector);

    if (isEnabling) {
      $selector.removeAttr(disabled);
    } else {
      $selector.attr(disabled, disabled);
    }
  }

  function getAuthToken() {
    return localStorage.getItem("auth-token");
  }

  function callAPI(url, method, data, callback) {
    $.ajax({
      beforeSend: function(request) {
        request.setRequestHeader("Authorization", `Bearer ${getAuthToken()}`);
      },
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback
    });
  }

})(app = window.app || {});