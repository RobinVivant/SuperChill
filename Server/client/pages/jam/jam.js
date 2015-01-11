
var loadedSounds = [];

var isSampleSelected = function(path){

  var jam = Jam.find({
    _id: Session.get("jamId"),
    tracks: {
      $elemMatch: {
        path: path,
        zouzou: localStorage.getItem("zouzouId")
      }
    }
  });
  return  jam.fetch().length ? true : false;
};

var playSound = function(elem, context){
  var id = Random.hexString(10);
  createjs.Sound.removeAllSounds();

  function loadHandler(event) {
    $(elem).velocity('stop');
    $(elem).css('opacity', 1);
    loadedSounds[context.path] = id;
    createjs.Sound.play(id);
  }

  if(!_.contains(loadedSounds, context.path) ) {
    $(elem).velocity({
      properties:{
        opacity: 0
      }, options:{
        duration:'100',
        loop: true
      }
    });
    createjs.Sound.on("fileload", loadHandler, context);
    createjs.Sound.registerSound(context.path, id);
  }else{
    createjs.Sound.play(loadedSounds[context.path]);
  }

};


var stopSound = function(elem){
  loadedSounds = [];

  $(elem).velocity('stop');
  $(elem).css('opacity', 1);
  createjs.Sound.removeAllSounds();
}

Template.jam.helpers({
  samples: function () {
    return isTablet ?Jam.findOne() : Samples.findOne();//Session.get("samples");
  },
  isTablet:function(){
    return isTablet;
  },
  isHeaderShown: function(){
    return Session.get('headerShown');
  },
  jamName:function(){
    return Session.get("jamName");
  },
  zouzouId: function(){
    return Session.get("zouzouId");
  },
  jamHeaderDragging: function(){
    return Session.get("jamHeaderMousePosInit") > -1 || (!Session.get('headerShown') && !Session.get("jamId") ) ? "jamHeaderDragging" : "";
  },
  jamHeaderPosition: function(){
    return Session.get("jamHeaderTop");
  },
  mainHeaderPosition: function(){
    return Session.get("jamHeaderTop")-$(window).height()+100 ;
  },
  windowHeight: function(){
    return $(window).height()-100 ;
  },
  nickname: function(){
    return Session.get('nickname');
  },
  jams: function(){
    return Jam.find({}, {fields:{
      _id:1,
      name:1
    }}).fetch();
  },
  jamListHeight: function(){
    return $(window).height()-2*100;
  }
});

Template.jamTree.helpers({
  isSampleSelected: function(){
    return  isSampleSelected(this.path) ? "sampleSelected": "";
  },
  zouzouId: function(){
    return Session.get("zouzouId");
  },
  groupSelected: function(){
    if( !this.childs )
      return;
    for( var i in this.childs ){
      if( isSampleSelected(this.childs[i].path) ){
        return "groupSelected";
      }
    }
  },
  ifIsGroup: function(){
    if(this.childs )
      return "groupHeader";
  }
});

Template.jam.events({
  'click .phoneSample': function (e, tmpl) {
    Jam.update({
      _id: Session.get('jamId')
    },{
      $addToSet:{
        tracks: {
          zouzou: localStorage.getItem("zouzouId"),
          path : this.path,
          name: this.name
        }
      }
    });
  },
  'click .sampleSelected .phoneSample': function (e, tmpl) {

    Jam.update({
      _id: Session.get('jamId')
    },{
      $pull:{
        tracks: {
          zouzou: localStorage.getItem("zouzouId"),
          path : this.path
        }
      }
    });
  },
  'click .groupHeaderSwitch': function (e, tmpl) {
    var elem = $(e.currentTarget).parent().find(".samplesContainer");

    if( !elem.is(":visible") ) {
      $(e.currentTarget).parent().velocity('stop').velocity({
        properties:{
          paddingBottom: ['0','26px']
        }, options:{
          duration:'300'
        }
      });
      elem.velocity('stop').velocity('slideDown',{
        duration: '300',
        queue: false
      });

      elem.velocity({
        properties:{
          marginTop: ['26px','0'],
          marginBottom: ['40px', '0']
        }, options:{
          duration:'300'
        }
      });

    }else {
      $(e.currentTarget).parent().velocity('stop').velocity({
        properties: {
          paddingBottom: ['26px', '0']
        }, options: {
          duration: '300'
        }
      });

      elem.velocity('stop').velocity('slideUp',{
        duration: '300',
        queue: false
      });

      elem.velocity({
        properties:{
          marginTop: ['0','26px'],
          marginBottom: ['0','40px']
        }, options:{
          duration:'300'
        }
      });
    }
  },
  'touchstart .playButton': function(e, tmpl) {
    playSound(e.currentTarget, this);
  },
  'mousedown .playButton': function(e, tmpl) {
    playSound(e.currentTarget, this);
  },
  'touchend .playButton': function(e, tmpl) {
    stopSound(e.currentTarget);
  },
  'mouseup .playButton': function(e, tmpl) {
    stopSound(e.currentTarget);
  },
  'touchstart .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') )
      return;
    Session.set("jamHeaderTop", 0);
    Session.set("jamHeaderMousePosInit", e.clientY);
  },
  'mousedown .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') )
      return;
    Session.set("jamHeaderTop", 0);
    Session.set("jamHeaderMousePosInit", e.clientY);
  },

  'click .jamHeader': function(e, tmpl) {
    if( !Session.get('headerShown') || !Session.get('jamId'))
      return;

    $('.jamHeader').velocity({
      properties:{
        top: 0
      }, options:{
        duration:'300',
        queue: false
      }
    });
    $('.mainHeader').velocity({
      properties:{
        top: -$(window).height()+'px'
      }, options:{
        duration:'300',
        complete: function(){
          Session.set("jamHeaderMousePosInit", -1);
          Session.set('headerShown',false);
        }
      }
    });
    window.history.replaceState(Session.get('jamName'), Session.get('jamName'), '/'+Session.get('jamId'));
  },
  'touchMove .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') || !Session.get('jamId' || Session.get("jamHeaderMousePosInit") == -1 ) )
      return;
    var pos = Session.get("jamHeaderMousePosInit");
    if(pos > -1 ){
      Session.set("jamHeaderTop", Math.min(Math.max(e.clientY - Session.get("jamHeaderMousePosInit"), 0), $(window).height()-100));
    }
  },
  'mousemove': function(e, tmpl) {
    if( Session.get('headerShown') || !Session.get('jamId') || Session.get("jamHeaderMousePosInit") == -1 )
      return;
    var pos = Session.get("jamHeaderMousePosInit");
    if(pos > -1 ){
      Session.set("jamHeaderTop", Math.min(Math.max(e.clientY - Session.get("jamHeaderMousePosInit"), 0), $(window).height()-100));
    }
  },
  'touchend .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') || !Session.get('jamId') )
      return;
    if( Session.get("jamHeaderTop" ) > $(window).height()/8 ){
      $('.jamHeader').velocity({
        properties:{
          top: $(window).height()-100+'px'
        }, options:{
          duration:'300',
          queue: false
        }
      });
      $('.mainHeader').velocity({
        properties:{
          top: 0
        }, options:{
          duration:'300',
          complete: function(){
            Session.set('headerShown',true);
          }
        }
      });
    }else{
      Session.set("jamHeaderMousePosInit", -1);
    }

  },
  'mouseup .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') || !Session.get('jamId') )
      return;
    if( Session.get("jamHeaderTop" ) > $(window).height()/8 ){
      $('.jamHeader').velocity({
        properties:{
          top: $(window).height()-100+'px'
        }, options:{
          duration:'300',
          queue: false
        }
      });
      $('.mainHeader').velocity({
        properties:{
          top: 0
        }, options:{
          duration:'300',
          complete: function(){
            Session.set('headerShown', true);
          }
        }
      });
    }else{
      Session.set("jamHeaderMousePosInit", -1);
    }

  },
  'blur #nickname': function(e, tmpl) {
    Session.set('nickname', e.currentTarget.value);
    Jam.update({
      _id: Session.get("jamId")
    },{
      $pull:{
        zouzous: {
          id : localStorage.getItem("zouzouId")
        }
      }
    }, function(error){
      Jam.update({
        _id: Session.get("jamId")
      },{
        $push:{
          zouzous: {
            id : localStorage.getItem("zouzouId"),
            name: e.currentTarget.value
          }
        }
      });
    });
  },

  'click .jamItem': function(e, tmpl) {
    Meteor.subscribe('jam', $(e.currentTarget).attr('data-id'), {
      onError: function(error){
        Router.go('/');
      },
      onReady: function(error, doc){

        Jam.update({
          _id: $(e.currentTarget).attr('data-id')
        },{
          $addToSet:{
            zouzous: {
              id : localStorage.getItem("zouzouId"),
              name: Session.get('nickname')
            }
          }
        });
        Session.set("jamName", Jam.find({_id: $(e.currentTarget).attr('data-id')}).fetch()[0].name);

      }
    });
    Session.set("jamId",$(e.currentTarget).attr('data-id'));
  }

});

Template.jam.created = function(){

  var nickname = "Anonymous";

  if( !this.data.jamId ){
    Session.set('headerShown', true);
    Session.set("jamHeaderMousePosInit", 666);
    Session.set("jamHeaderTop", $(window).height()-100);
    Session.set("jamName", "No Jam selected");
  }

  Session.set("jamId",this.data.jamId);

  if( !localStorage.getItem("zouzouId") ){
    localStorage.setItem("zouzouId", Random.hexString(6));
  }else if(this.data.jamId){
    var zouzous = Jam.find({
      _id: this.data.jamId
    },{
      fields:{
        zouzous: 1
      }
    }).fetch()[0].zouzous;

    for( var i in zouzous){
      if( zouzous[i].id == localStorage.getItem("zouzouId")){
        nickname = zouzous[i].name;
        break;
      }
    }
    Jam.update({
      _id: this.data.jamId
    },{
      $addToSet:{
        zouzous: {
          id : localStorage.getItem("zouzouId"),
          name: nickname
        }
      }
    });
  }
  Session.set('nickname', nickname);
  Session.set("zouzouId",localStorage.getItem("zouzouId"));


  /*
   if(Meteor.isCordova){
   cordova.plugins.barcodeScanner.scan(
   function (result) {
   alert("We got a barcode\n" +
   "Result: " + result.text + "\n" +
   "Format: " + result.format + "\n" +
   "Cancelled: " + result.cancelled);
   },
   function (error) {
   alert("Scanning failed: " + error);
   }
   );
   }
   */



};

Template.jam.destroyed = function(){
  //Meteor.unsubscribe('samples');
};

