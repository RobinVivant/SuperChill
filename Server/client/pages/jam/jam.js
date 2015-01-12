
var loadedSounds = [];

var isSampleSelected = function(path){
  return  JamTracks.findOne({
    path: path,
    zouzou: localStorage.getItem("zouzouId")
  });
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
};

Template.jam.helpers({
  samples: function () {
    return isTablet ? Jam.findOne() : Samples.findOne();
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
    return $(window).height()-200;
  },
  jamItemSelected: function(){
    return this._id === Session.get('jamId') ? "jamItemSelected" : "";
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

function onClickJamHeader() {
  if( !Session.get('headerShown') || !Session.get('jamId'))
    return;

  Session.set('headerShown',false);

  $('.jamHeader').velocity({
    properties:{
      top: 0
    }, options:{
      duration:'250',
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
      }
    }
  });
}

function onDragJamHeader(clientY) {
  if( Session.get('headerShown') || !Session.get('jamId') || Session.get("jamHeaderMousePosInit") == -1 )
    return;
  var pos = Session.get("jamHeaderMousePosInit");
  if(pos > -1 ){
    Session.set("jamHeaderTop", Math.min(Math.max(clientY - Session.get("jamHeaderMousePosInit"), 0), $(window).height()-100));
  }
}

function onDragEndJamHeader() {
  if( Session.get('headerShown') || !Session.get('jamId') || Session.get("jamHeaderMousePosInit") == -1 )
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
}

Template.jam.events({
  'click .phoneSample' : function (e, tmpl) {
    if( $(e.currentTarget).parent().hasClass('sampleSelected')){
      JamTracks.remove({
        _id: JamTracks.findOne({path: this.path})._id
      });
    }else{
      JamTracks.insert({
        zouzou: localStorage.getItem("zouzouId"),
        jamId: Session.get('jamId'),
        path : this.path
      });
    }

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
  'click .jamHeader': function(e, tmpl) {
    onClickJamHeader();
  },
  'touchstart .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') ){
      onClickJamHeader();
    }else{
      Session.set("jamHeaderTop", 0);
      Session.set("jamHeaderMousePosInit", e.originalEvent.changedTouches[0].clientY );
    }
    e.preventDefault();
  },
  'mousedown .jamHeader': function(e, tmpl) {
    if( Session.get('headerShown') )
      return;
    Session.set("jamHeaderTop", 0);
    Session.set("jamHeaderMousePosInit", e.clientY);
  },
  'touchmove ': function(e, tmpl) {
    onDragJamHeader(e.originalEvent.changedTouches[0].clientY);
    //e.preventDefault();
  },
  'mousemove': function(e, tmpl) {
    onDragJamHeader(e.clientY);
    //e.preventDefault();
  },
  'touchend': function(e, tmpl) {
    onDragEndJamHeader();
  },
  'mouseup': function(e, tmpl) {
    onDragEndJamHeader();
  },
  'keyup #nickname': function(e, tmpl) {
    Session.set('nickname', e.currentTarget.value);
    Zouzous.update({
      _id: Zouzous.findOne({hexId:Session.get('zouzouId')})._id
    },{
      $set:{
        nickname: e.currentTarget.value
      }
    });
  },
  'click .jamItem': function(e, tmpl) {
    Session.set("jamId",$(e.currentTarget).attr('data-id'));
    Session.set("jamName", $(e.currentTarget).html());
  }
});

Template.jam.created = function(){

  Session.set('nickname', 'Anonymous');

  if( !Session.get("jamId") ){
    Session.set('headerShown', true);
    Session.set("jamHeaderMousePosInit", 666);
    Session.set("jamHeaderTop", $(window).height()-100);
    Session.set("jamName", "No Jam");
  }

  Tracker.autorun(function () {
    window.history.replaceState(Session.get('jamName'), Session.get('jamName'), '/'+Session.get('jamId'));
  });
  Tracker.autorun(function () {
    Meteor.subscribe('jam-tracks', Session.get('jamId'));
    Meteor.subscribe('jam', Session.get('jamId'), {
      onError: function(error){
        Router.go('/');
      },
      onReady: function(doc){
        Session.set("jamName", Jam.findOne({_id: Session.get('jamId')}).name);
        var zouzou = Zouzous.findOne({jamId: Session.get("jamId")});
        if (!zouzou) {
          Zouzous.insert({
            jamId: Session.get("jamId"),
            hexId: Session.get("zouzouId"),
            nickname: Session.get("nickname")
          });
        } else {
          Session.set("nickname", zouzou.nickname);
        }
      }
    });
  });

};


