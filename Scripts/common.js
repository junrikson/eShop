// loading Ajax

$(document).ajaxStart(function ()
{
    $("#loading").show();
});
$(document).ajaxComplete(function ()
{
    $("#loading").hide();
});

// Populate Sidebar

$(document).ready(function ()
{
    $('.sidebar-menu').tree();
});


// Search Function on top

$(function ()
{
    $("body").on('click keypress', function ()
    {
        ResetThisSession();
    });
});

// Automatic Log Out

var isExist = true;
var logOutCounter = 0;
var tempSecondTick = 0;

Cookies.set("timeInSecondsAfterSessionOut", 1800);
ResetThisSession();


function ResetThisSession()
{
    Cookies.set("secondTick", 0);
    tempSecondTick = 1;
    logOutCounter = 0;
}

function StartThisSessionTimer()
{
    logOutCounter++;

    if (logOutCounter > 3)
    {
        logOutCounter = 0;

        if (tempSecondTick === parseInt(Cookies.get("secondTick")))
        {
            isExist = false;
        }

        tempSecondTick = parseInt(Cookies.get("secondTick"));
    }

    if (!isExist)
    {
        Cookies.set("secondTick", parseInt(Cookies.get("secondTick")) + 1);
    }
    var timeLeft = Math.floor((parseInt(Cookies.get("timeInSecondsAfterSessionOut")) - parseInt(Cookies.get("secondTick"))) / 60);

    if ((parseInt(Cookies.get("timeInSecondsAfterSessionOut")) - parseInt(Cookies.get("secondTick"))) <= 0)
    {
        $("#spanTimeLeft").html("0:0");
    }
    else
    {
        $("#spanTimeLeft").html(timeLeft.toString() + ":" + ((parseInt(Cookies.get("timeInSecondsAfterSessionOut")) - parseInt(Cookies.get("secondTick"))) % 60).toString());
    }

    if (parseInt(Cookies.get("secondTick")) > parseInt(Cookies.get("timeInSecondsAfterSessionOut")))
    {
        Cookies.set("secondTick", 0);
        clearTimeout(tick);
        $('#logoutForm').submit();
        return;
    }
    tick = setTimeout("StartThisSessionTimer()", 1000);
}

StartThisSessionTimer();


// Full Screen Button

document.getElementById('fullscreen-btn').addEventListener('click', () =>
{
    if (screenfull.enabled)
    {
        screenfull.toggle();
    }
});


// Toggle Right Sidebar 

function handleChange(checkbox)
{
    var checkboxStatus = false;
    if (checkbox.checked === true)
    {
        checkboxStatus = true;
    }
    var data = { status: checkboxStatus };
    var token = $('input[name="__RequestVerificationToken"]').val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    var options = {};
    options.url = "/Profile/ToggleSidebar";
    options.type = "POST";
    options.headers = headers;
    options.data = JSON.stringify(data);
    options.contentType = "application/json";
    options.dataType = "json";
    options.success = function (msg)
    {
        window.location.href = '/';
    };
    options.error = function ()
    {
        window.location.href = '/';
    };
    $.ajax(options);
}

// Change Skin

function skinChange(skin)
{
    var data = { color: skin };
    var token = $('input[name="__RequestVerificationToken"]').val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    var options = {};
    options.url = "/Profile/SkinChange";
    options.type = "POST";
    options.headers = headers;
    options.data = JSON.stringify(data);
    options.contentType = "application/json";
    options.dataType = "json";
    options.success = function (msg)
    {
        window.location.href = '/';
    };
    options.error = function ()
    {
        window.location.href = '/';
    };
    $.ajax(options);
}


// Chat Room

function addChatTag(usernameTag)
{
    $("#message").val($("#message").val() + ' @' + usernameTag + ' ');
    $('#message').focus();
}

var isOpen = false;

function openChat()
{
    document.getElementById("ChatContainer").style.display = "block";

    $('#discussion').animate({
        scrollTop: $('#discussion').prop("scrollHeight")
    }, 300);

    $('#ChatButton').removeClass("have-message");

    $('#message').focus();

    isOpen = true;
}

function closeChat()
{
    document.getElementById("ChatContainer").style.display = "none";

    isOpen = false;
}

$(function ()
{
    // Reference the auto-generated proxy for the hub.  
    var tempName = '';
    var chat = $.connection.chatHub;
    var currentUsername = $("input#username").val();
    // Create a function that the hub can call back to display messages.
    chat.client.updateUserList = function (users)
    {
        var userLists = '';

        for (i = 0; i < users.length; i++)
        {
            userLists = userLists + '<li><a class="username-tag" onClick="addChatTag(\'' + users[i].UserName + '\')" href="#">' + users[i].FullName + '</a></li>';
        }

        $('#contactsList').html(userLists);
    };

    chat.client.addNewMessageToPage = function (name, message, timestamp, username)
    { 
        if (!isOpen)
        {
            $('#ChatButton').addClass("have-message");
            if(message.toLowerCase().indexOf('@' + currentUsername.toLowerCase()) !== -1)
            {
                openChat();
                document.getElementById('chatNotification').play();
            }
        }

        if (currentUsername === username)
        {
            if (tempName === name)
            {
                $('#discussion').append(
                    '<div class="text-right">' +
                    '   <p class="chat-style-right">' + escapeHtml(message) + '</p>' +
                    '</div>');
            }
            else
            {
                $('#discussion').append(
                    '<div class="text-right">' +
                    '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                    '       <span class="direct-chat-name pull-right"><a class="username-tag" onClick="addChatTag(\'' + username + '\')" href="#">' + name + '</a></span>' +
                    '       <span class="direct-chat-timestamp pull-left">' + timestamp + '</span>' +
                    '   </div>' +
                    '   <p class="chat-style-right">' + escapeHtml(message) + '</p>' +
                    '</div>');
            }
        }
        else
        {
            if (tempName === name)
            {
                $('#discussion').append(
                    '<div class="text-left">' +
                    '   <p class="chat-style-left">' + escapeHtml(message) + '</p>' +
                    '</div>');
            }
            else
            {
                $('#discussion').append(
                    '<div class="text-left">' +
                    '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                    '       <span class="direct-chat-name pull-left"><a class="username-tag" onClick="addChatTag(\'' + username + '\')" href="#">' + name + '</a></span>' +
                    '       <span class="direct-chat-timestamp pull-right">' + timestamp + '</span>' +
                    '   </div>' +
                    '   <p class="chat-style-left">' + escapeHtml(message) + '</p>' +
                    '</div>');
            }
        }

        tempName = name;

        $('#discussion').animate({
            scrollTop: $('#discussion').prop("scrollHeight")
        },0);
    };
    // Set initial focus to message input box.  
    $('#message').focus();
    // Start the connection.
    $.connection.hub.start().done(function ()
    {
        $('#sendmessage').click(function ()
        {
            if ($('#message').val() !== "")
            {
                // Call the Send method on the hub. 
                chat.server.send($('#message').val());
                // Clear text box and reset focus for next comment. 
                $('#message').val('').focus();
            }
        });

        $('#message').bind("enterKey", function (e)
        {
            if ($('#message').val() !== "")
            {
                // Call the Send method on the hub. 
                chat.server.send($('#message').val());
                // Clear text box and reset focus for next comment. 
                $('#message').val('').focus();
            }
        });
        $('#message').keyup(function (e)
        {
            if (e.keyCode === 13)
            {
                $(this).trigger("enterKey");
            }
        });
    });
});

var chatIndex = 0;

function GetChatData()
{
    var currentUsername = $("input#username").val();

    var old_height = $('#discussion')[0].scrollHeight;

    $.ajax({
        type: 'GET',
        url: '/Chats/GetData',
        data: { "chatIndex": chatIndex },
        dataType: 'json',
        success: function (data)
        {
            if (data !== null)
            {
                for (var i = 0; i < data.length; i++)
                {
                    chatIndex = data[i].Id;

                    if (currentUsername === data[i].UserName)
                    {
                        if ((i + 1) === data.length)
                        {
                            $('#discussion').prepend(
                                '<div class="text-right">' +
                                '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                                '       <span class="direct-chat-name pull-right"><a class="username-tag" onClick="addChatTag(\'' + data[i].UserName + '\')" href="#">' + data[i].FullName + '</a></span>' +
                                '       <span class="direct-chat-timestamp pull-left">' + data[i].TimeStamp + '</span>' +
                                '   </div>' +
                                '   <p class="chat-style-right">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                        else if (data[i + 1].FullName === data[i].FullName)
                        {
                            $('#discussion').prepend(
                                '<div class="text-right">' +
                                '   <p class="chat-style-right">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                        else
                        {
                            $('#discussion').prepend(
                                '<div class="text-right">' +
                                '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                                '       <span class="direct-chat-name pull-right"><a class="username-tag" onClick="addChatTag(\'' + data[i].UserName + '\')" href="#">' + data[i].FullName + '</a></span>' +
                                '       <span class="direct-chat-timestamp pull-left">' + data[i].TimeStamp + '</span>' +
                                '   </div>' +
                                '   <p class="chat-style-right">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                    }
                    else
                    {
                        if ((i + 1) === data.length)
                        {
                            $('#discussion').prepend(
                                '<div class="text-left">' +
                                '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                                '       <span class="direct-chat-name pull-left"><a class="username-tag" onClick="addChatTag(\'' + data[i].UserName + '\')" href="#">' + data[i].FullName + '</a></span>' +
                                '       <span class="direct-chat-timestamp pull-right">' + data[i].TimeStamp + '</span>' +
                                '   </div>' +
                                '   <p class="chat-style-left">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                        else if (data[i + 1].FullName === data[i].FullName)
                        {
                            $('#discussion').prepend(
                                '<div class="text-left">' +
                                '   <p class="chat-style-left">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                        else
                        {
                            $('#discussion').prepend(
                                '<div class="text-left">' +
                                '   <div class="direct-chat-info clearfix" style="margin-top: 5px;">' +
                                '       <span class="direct-chat-name pull-left"><a class="username-tag" onClick="addChatTag(\'' + data[i].UserName + '\')" href="#">' + data[i].FullName + '</a></span>' +
                                '       <span class="direct-chat-timestamp pull-right">' + data[i].TimeStamp + '</span>' +
                                '   </div>' +
                                '   <p class="chat-style-left">' + escapeHtml(data[i].Message) + '</p>' +
                                '</div>');
                        }
                    }
                    $('#discussion').scrollTop($('#discussion')[0].scrollHeight - old_height); 
                }
            }
        }
    });
}

$(document).ready(function ()
{
    GetChatData();

    $('#discussion').scroll(function ()
    {
        if ($('#discussion').scrollTop() ===
            $('#discussion').height() - $('#discussion').height())
        {
            GetChatData();
        }
    });
});


var entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '/': '&#x2F;',
    '`': '&#x60;',
    '=': '&#x3D;'
};

function escapeHtml(string)
{
    temp = String(string).replace(/[&<>"'`=\/]/g, function (s)
    {
        return entityMap[s];
    });

    return convertChats(temp);
}

function convertChats(text)
{
    var exp = /(\b(https?|ftp|file):&#x2F;&#x2F;[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig;
    text = text.replace(/@:e\d+/g, function (x)
    {
        x = x.replace('@:', '');
        return ' <img width="30" height="30" alt="smiley" src="/Content/emoji_png/' + x + '.png" /> ';
    });
    return text.replace(exp, "<a style=\"color: #FF0000;\" target=\"_blank\" href='$1'>$1</a>");
}
