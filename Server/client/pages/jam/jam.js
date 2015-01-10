
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

var playSound = function(context){
  var id = Random.hexString(10);
  createjs.Sound.removeAllSounds();

  function loadHandler(event) {
    loadedSounds[context.path] = id;
    createjs.Sound.play(id);
  }

  if(!_.contains(loadedSounds, context.path) ) {
    createjs.Sound.on("fileload", loadHandler, context);
    createjs.Sound.registerSound(context.path, id);
  }else{
    createjs.Sound.play(loadedSounds[context.path]);
  }

};


var stopSound = function(){
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
  isGroupSelected: function(){
    for( var i in this.childs ){
      if( isSampleSelected(this.childs[i].path) ){
        return "groupSelected";
      }
    }
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
  'click .sampleGroup': function (e, tmpl) {
    var elem = $(e.currentTarget).parent().find(".samplesContainer");

    if( !elem.is(":visible") ) {
      elem.velocity('stop').velocity('slideDown',{
        duration: '300'
      });
    }else {
      elem.velocity('stop').velocity('slideUp',{
        duration: '300'
      });
    }
  },
  'touchstart .playButton': function(e, tmpl) {
    playSound(this);
  },
  'mousedown .playButton': function(e, tmpl) {
    playSound(this);
  },
  'touchend .playButton': function(e, tmpl) {
    stopSound();
  },
  'mouseup .playButton': function(e, tmpl) {
    stopSound();
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

