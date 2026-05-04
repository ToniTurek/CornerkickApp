let webSocket;

function chat(iptChatMessage, btnChatSend, tblBdyChat, btnChatLogout, btnChatLogin, chatContainer, chatContainerInner, sUserName, sColor, chatMemberContainer, bMobile) {
  var stateLabel = document.getElementById("stateLabel");

  const scheme = document.location.protocol === "https:" ? "wss" : "ws";
  const port = document.location.port ? (":" + document.location.port) : "";
  const connectionUrl = scheme + "://" + document.location.hostname + port + "/ws";

  function openConnection(force = false) {
    $.ajax({
      url: '/Member/GetChatHistory',
      type: "GET",
      dataType: "JSON",
      cache: false,
      contentType: "application/json; charset=utf-8",
      success: function (ret) {
        // Print chat history
        for (var i = 0; i < ret.lt_chat_hist.length; i++) {
          printMessage(ret.lt_chat_hist[i], !bMobile)
        }

        // Open websocket connection
        if (force) {
          webSocket = new WebSocket(connectionUrl + "/?force=true");
        } else {
          webSocket = new WebSocket(connectionUrl);
        }

        webSocket.onopen = function (event) {
          updateState();
        };
        webSocket.onclose = function (event) {
          updateState();
        };
        webSocket.onmessage = function (message) {
          printMessage(JSON.parse(message.data), !bMobile);
        };
      }
    });
  }

  openConnection();

  function updateState() {
    //console.log("updateState");
    function disable() {
      iptChatMessage.disabled = true;
      iptChatMessage.style.display = "none";
      btnChatSend.disabled = true;
      btnChatSend.style.display = "none";
      btnChatLogout.disabled = true;
      btnChatLogout.style.display = "none";
      tblBdyChat.style.display = "none";
      chatMemberContainer.style.display = "none";
      chatContainer.style.left = "";
      chatContainer.style.right = (document.body.clientWidth - chatContainer.parentNode.offsetLeft - chatContainer.parentNode.offsetWidth).toString() + "px";
      chatContainerInner.style.width = "fit-content";
      chatContainer.style.width = "fit-content";
    }
    function enable() {
      const iOnlineListWidth = 300;

      iptChatMessage.disabled = false;
      iptChatMessage.style.display = "inline";
      btnChatSend.disabled = false;
      btnChatSend.style.display = "inline";
      btnChatLogout.disabled = false;
      btnChatLogout.style.display = "inline";
      tblBdyChat.style.display = "inline";
      chatMemberContainer.style.display = "inline";
      chatContainer.style.left = chatContainer.parentNode.offsetLeft.toString() + "px";
      chatContainer.style.right = "";
      if (chatContainer.parentNode.offsetWidth < 600) {
        chatContainerInner.style.width = chatContainer.parentNode.offsetWidth.toString() + "px";
      } else {
        chatContainerInner.style.width = Math.max(chatContainer.parentNode.offsetWidth - 300, 600).toString() + "px";
      }
      chatContainer.style.width = (chatContainerInner.offsetWidth + 2 + iOnlineListWidth).toString() + "px";

      // Draw member table
      $.ajax({
        url: '/Member/GetChatMember',
        type: "GET",
        dataType: "JSON",
        cache: false,
        contentType: "application/json; charset=utf-8",
        success: function (ret) {
          const tblBdyMember = chatMemberContainer.getElementsByTagName("tbody")[0];

          for (var i = 0; i < ret.lt_member.length; i++) {
            var row = tblBdyMember.insertRow();
            var imgBall = document.createElement("img");
            imgBall.src = "/Content/Icons/ball_transparent_inner.png";
            imgBall.width = 12;
            //var iDot = document.createElement("i");
            //iDot.className = "fa fa-circle";
            var cell0 = row.insertCell();
            if (ret.lt_member[i].min_last_active < 1) {
              cell0.title = "aktiv";
              imgBall.style.backgroundColor = "#76ff03";
              //iDot.style.color = "green";
            } else if (ret.lt_member[i].min_last_active < 5) {
              cell0.title = ret.lt_member[i].min_last_active.toFixed(0) + " Min.";
              //iDot.style.color = "yellow";
              imgBall.style.backgroundColor = "yellow";
            } else if (ret.lt_member[i].min_last_active < 15) {
              cell0.title = ret.lt_member[i].min_last_active.toFixed(0) + " Min.";
              //iDot.style.color = "red";
              imgBall.style.backgroundColor = "red";
            } else {
              cell0.title = "inaktiv";
              //iDot.style.color = "red";
              imgBall.style.backgroundColor = "red";
            }
            //cell0.appendChild(iDot);
            cell0.appendChild(imgBall);

            var cell1 = row.insertCell();
            cell1.style.paddingLeft = "4px";
            //cell1.style.color = "rgb(" + ret.lt_member[i].color + ")";
            cell1.innerHTML = ret.lt_member[i].name;
          }
        }
      });
    }

    btnChatLogin.disabled = true;
    btnChatLogin.style.display = "none";

    if (!webSocket) {
      disable();
    } else {
      //console.log("webSocket.readyState: " + webSocket.readyState + " (" + WebSocket.CLOSED + ", " + WebSocket.CLOSING + ", " + WebSocket.CONNECTING + ", " + WebSocket.OPEN + ")");
      switch (webSocket.readyState) {
        case WebSocket.CLOSED:
          stateLabel.innerHTML = "Closed";
          disable();
          btnChatLogin.disabled = false;
          btnChatLogin.style.display = "inline";
          tblBdyChat.innerHTML = '';
          break;
        case WebSocket.CLOSING:
          stateLabel.innerHTML = "Closing...";
          disable();
          break;
        case WebSocket.CONNECTING:
          stateLabel.innerHTML = "Connecting...";
          enable();
          break;
        case WebSocket.OPEN:
          stateLabel.innerHTML = "Open: " + connectionUrl;
          enable();
          break;
        default:
          stateLabel.innerHTML = "Unknown WebSocket State: " + htmlEscape(webSocket.readyState);
          disable();
          break;
      }
    }
  }

  function printMessage(data_json, print_time) {
    const data_date = new Date(data_json.date);
    const sTime = data_date.toLocaleTimeString(undefined, {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit"
    });

    // Salary row
    var rowChat = tblBdyChat.insertRow();

    if (print_time) {
      var cell0 = rowChat.insertCell();
      cell0.style.textAlign = "center";
      cell0.innerText = sTime;
    }

    var cell1 = rowChat.insertCell();
    cell1.style.paddingLeft = "4px";
    cell1.style.fontWeight = "bold";
    cell1.style.textAlign = "center";
    cell1.style.color = "rgb(" + data_json.color + ")";
    cell1.innerText = data_json.user;

    var cell2 = rowChat.insertCell();
    cell2.style.whiteSpace = "break-spaces";
    cell2.style.paddingLeft = "4px";
    cell2.style.textAlign = "left";
    cell2.innerHTML = htmlEscape(data_json.message);

    var tblBdyChatContainer = tblBdyChat.parentNode.parentNode;
    tblBdyChatContainer.scrollTop = tblBdyChatContainer.scrollHeight;
    //tblBdyChatContainer.scrollTo(0, tblBdyChatContainer.scrollHeight);
  }

  btnChatLogin.onclick = function () {
    openConnection(true);
    iptChatMessage.focus();
  };

  btnChatLogout.onclick = function () {
    if (!webSocket || webSocket.readyState !== WebSocket.OPEN) {
      return;
    }
    //webSocket.send(JSON.stringify({ date: new Date(), user: sUserName, color: sColor, message: "hat den Chat verlassen..." }));
    tblBdyChat.innerHTML = '';
    webSocket.close(1000, "Closing from client");
  };

  btnChatSend.addEventListener("click", function () {
    sendMessage(iptChatMessage.value);
    iptChatMessage.value = '';
    iptChatMessage.focus();
  });

  function sendMessage(message) {
    webSocket.send(JSON.stringify({ date: new Date(), user: sUserName, color: sColor, message: message }));
  }

  function htmlEscape(str) {
    return str.toString()
      .replace(/&/g, '&amp;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
  }
}
