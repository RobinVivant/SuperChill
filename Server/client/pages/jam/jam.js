
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
    });//css('background-color', '#'+h+m+s);
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
  jamName:function(){
    return Session.get("jamName");
  },
  zouzouId: function(){
    return Session.get("zouzouId");
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
      _id: tmpl.data.jamId
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
      _id: tmpl.data.jamId
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
      $(e.currentTarget).parent().velocity({
        properties:{
          paddingTop: '26px',
          paddingBottom: '0'
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
          marginTop: '26px',
          marginBottom: '40px'
        }, options:{
          duration:'300'
        }
      });

    }else {
      $(e.currentTarget).parent().velocity({
        properties: {
          paddingTop: '26px',
          paddingBottom: '26px'
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
          marginTop: '0',
          marginBottom: '0'
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
  }
});

Template.jam.created = function(){
  if( !localStorage.getItem("zouzouId") ){
    localStorage.setItem("zouzouId", Random.hexString(6));
  }

  Session.set("zouzouId",localStorage.getItem("zouzouId"));

  Jam.update({
    _id: this.data.jamId
  },{
    $addToSet:{
      zouzous: {
        id : localStorage.getItem("zouzouId"),
        name: "Anonymous"
      }
    }
  });

};

Template.jam.destroyed = function(){
  //Meteor.unsubscribe('samples');
};

