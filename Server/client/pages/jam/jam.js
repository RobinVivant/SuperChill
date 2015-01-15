
var loadedSounds = [];

var rollinBackHeader = false;

var wasPlaying = false;

var playTarget;

var isSampleSelected = function(path){
  return  JamTracks.findOne({
    path: path,
    zouzou: localStorage.getItem("zouzouId")
  });
};

var playSound = function(elem, context){
  var id = Random.hexString(10);
  createjs.Sound.removeAllSounds();

  playTarget = elem;

  function loadHandler(event) {
    loadedSounds[context.path] = id;
    createjs.Sound.play(id);
    $(elem).velocity('stop');
    Meteor.defer(function(){
      $(elem).css('background-color','#'+localStorage.getItem('zouzouId'));
    });
  }

  if(!_.contains(loadedSounds, context.path) ) {
    $(elem).velocity('stop').velocity({
      properties:{
        backgroundColor: '#d2d2d2'
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
  $(elem).velocity('stop');
  Meteor.defer(function(){
    $(elem).css('background-color','#'+localStorage.getItem('zouzouId'));
  });
  loadedSounds = [];
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
    if(this.childs)
      return "groupHeader";
  }
});

function onClickJamHeader() {
  if( !Session.get('headerShown') || !Session.get('jamId'))
    return;

  Session.set('headerShown',false);
  rollinBackHeader = true;

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
        rollinBackHeader = false;
      }
    }
  });
}

function onDragJamHeader(clientY) {
  if( rollinBackHeader || Session.get('headerShown') || !Session.get('jamId') || Session.get("jamHeaderMousePosInit") == -1 )
    return;
  var pos = Session.get("jamHeaderMousePosInit");
  if(pos > -1 ){
    Session.set("jamHeaderTop", Math.min(Math.max(clientY - Session.get("jamHeaderMousePosInit"), 0), $(window).height()-100));
  }
}

function onDragEndJamHeader() {
  if( rollinBackHeader || Session.get('headerShown') || !Session.get('jamId') || Session.get("jamHeaderMousePosInit") == -1 )
    return;
  if( Session.get("jamHeaderTop" ) > $(window).height()/8 ){
    rollinBackHeader = true;
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
          rollinBackHeader = false;
        }
      }
    });
  }else{
    Session.set("jamHeaderMousePosInit", -1);
  }
}

Template.jam.events({
  'click .trackContainer' : function (e, tmpl) {
    if( wasPlaying)
      return;
    if( $(e.currentTarget).hasClass('sampleSelected')){
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

      elem.velocity('stop').velocity({
        properties: {
          marginTop: [0, $(window).height()+'px']
        }, options: {
          duration: 300
        }
      });

      $(e.currentTarget).parent().addClass('GroupOpened');
      $(e.currentTarget).parent().parent().css('position', 'absolute');
      elem.show();
      elem.css('height', 'auto');


      $(e.currentTarget).parent().parent().css('height', elem.parent().height()+10+'px');

      $(e.currentTarget).parent().velocity('stop').velocity({
        properties: {
          top: [0, $(e.currentTarget).parent().offset().top-110+'px']
        }, options: {
          duration: 100
        }
      });

    }else{
      elem.velocity({
        properties: {
          height: [0, elem.height()+'px']
        }, options: {
          duration: 300,
          display: "none"
        }
      });
      $(e.currentTarget).parent().velocity('reverse',{
        complete:function(){
          $(e.currentTarget).parent().removeClass('GroupOpened');
          $(e.currentTarget).parent().parent().css('position', '');
          $(e.currentTarget).parent().parent().css('height', 'auto');
        }
      });
    }
  },
  'touchmove #samples .groupHeader': function(e, tmpl) {
    /*
     $('.groupHeader').css('background-color',Session.get('hexColorBg'));
     $('.groupHeader').css('opacity', e.originalEvent.changedTouches[0].clientX / $('.groupHeader').width());
     */
  },
  'touchstart .playButton': function(e, tmpl) {
    wasPlaying = true;
    playSound(e.currentTarget, this);
  },
  'mousedown .playButton': function(e, tmpl) {
    wasPlaying = true;
    playSound(e.currentTarget, this);
  },
  'touchend': function(e, tmpl) {
    onDragEndJamHeader();
    if(wasPlaying){
      stopSound(playTarget);
    }
    wasPlaying = false

  },
  'mouseup': function(e, tmpl) {
    onDragEndJamHeader();
    if(wasPlaying){
      stopSound(playTarget);
    }
    Meteor.defer(function(){
      wasPlaying = false
    });
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
    Session.set("jamName", "No Jam");
  }

  Session.set("jamHeaderTop", $(window).height()-100);


  Tracker.autorun(function () {
    Session.set('hexColorBg','#'+('000000' + (('0xffffff' ^ ('0x'+localStorage.getItem('zouzouId'))).toString(16))).slice(-6));
  });


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


