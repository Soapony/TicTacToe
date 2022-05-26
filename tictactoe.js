var mark="";
var name="";
var turnMark;
var waitMsg="Wait for another player to join. Check 'Try Game' intermittently to see if someone paired up with you.\nPlease do not spam.";
var marks=[" "," "," "," "," "," "," "," "," "];
var response="";
var gameid="";


function startGame() {
    name=document.getElementById("input").value;
    const url="http://localhost:8080/pairme?player="+name;
    const httpRequest = new XMLHttpRequest();
    httpRequest.open('GET', url);
    httpRequest.send();
    httpRequest.onload = function(){
        response=httpRequest.responseText;
        console.log(response);
        if(response == "wait"){
            setMessage(waitMsg);
        }
        else{
            var splits=response.split(" ");
            gameid=splits[2];
            mark=splits[0];
            var msg="Great "+name+", you are playing with "+splits[1]+". Your pieces are "+splits[0]+". Good luck!"; 
            setMessage(msg);
            document.getElementById("tryGame").style.display="none";
            console.log("gameid: "+gameid+" mark: "+mark);
            if(mark=="X"){
                document.getElementById("sendMove").style.display="inline-block";
            }else{
                document.getElementById("getMove").style.display="inline-block";
            }
        }
    }
    document.getElementById("quitGame").style.display="inline-block";
}

function sendMove(){
    var moves="";
    for(var i=0;i<9;i++){
        if(marks[i] == " "){
            moves+="0";
        }else{
            moves+=marks[i];
        }
    }
    var url="http://localhost:8080/mymove?player="+name+"&id="+gameid+"&move="+moves;
    console.log("url: "+url);
    var httpRequest = new XMLHttpRequest();
    httpRequest.open('GET', url);
    httpRequest.send();
    httpRequest.onload = function(){
        response=httpRequest.responseText;
        console.log(response);
    }
    document.getElementById("sendMove").style.display="none";
    document.getElementById("getMove").style.display="inline-block";
}

function getMove(){
    var url="http://localhost:8080/theirmove?player="+name+"&id="+gameid;
    console.log("url: "+url);
    var httpRequest = new XMLHttpRequest();
    httpRequest.open('GET', url);
    httpRequest.send();
    httpRequest.onload = function(){
        response=httpRequest.responseText;
        console.log(response);
        var tmp=0;
        var tmpMarks=[" "," "," "," "," "," "," "," "," "];
        for(var i=0;i<9;i++){
            if(response[i] != "0"){
                tmpMarks[i]=response[i];
                console.log(tmpMarks);
            }
            if(tmpMarks[i] != marks[i]){
                tmp=1;
            }
        }
        if(tmp == 1){
            for(var i=0;i<9;i++){
                marks[i]=tmpMarks[i];
            }
            console.log("marks: "+marks);
            refresh();
            document.getElementById("sendMove").style.display="inline-block";
            document.getElementById("getMove").style.display="none";
        }
        else{
            console.log("no change");
        }
    }
    //setMessage(response);
}

function quitGame(){
    var url="http://localhost:8080/quit?player="+name+"&id="+gameid;
    console.log("url: "+url);
    var httpRequest = new XMLHttpRequest();
    httpRequest.open('GET', url);
    httpRequest.send();
    httpRequest.onload = function(){
        response=httpRequest.responseText;
        console.log(response);
    }
    //setMessage(response);
    clearBox();
    setbtn();
    document.getElementById("tryGame").style.display="inline-block";
}

function refresh(){
    for(var i=0;i<9;i++){
        document.getElementById("b"+(i+1)).innerText=marks[i];
    }
}

function setMessage(msg) {
    document.getElementById("message").innerText = msg;
}

function reBoard(bId) {
    marks[bId-1]=mark;
    for(var i=0;i<9;i++){
        document.getElementById("b" + (i+1)).innerText = marks[i];
    }
    console.log(marks);
}

function clearBox() {
    for(var i=0;i<9;i++){
        marks[i]=" ";
        document.getElementById("b" + (i+1)).innerText = marks[i];
    }
}

function setbtn(){
    document.getElementById("quitGame").style.display="none";
    document.getElementById("getMove").style.display="none";
    document.getElementById("sendMove").style.display="none";
}