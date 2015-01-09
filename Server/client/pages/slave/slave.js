
currentZouzou = "";

Template.slave.helpers({
  samples: function () {
    return isTablet ? Jam.findOne() : Samples.findOne();
  },
  isTablet:function(){
    return isTablet;
  },
  isZouzouEq:function(z){
    console.log(currentZouzou === z, currentZouzou, z);
    return currentZouzou === z;
  },
  saveCurrentZouzou:function(){
    currentZouzou = this.id;
  }
});

Template.slaveTree.helpers({
  isSampleSelected: function(){

    var jam = Jam.find({
      _id: Session.get("jamId"),
      tracks: {
        $elemMatch: {
          path: this.path,
          zouzou: localStorage.getItem("zouzouId")
        }
      }
    });
    return  jam.fetch().length ? "sampleSelected": "";
  }
});

Template.slave.events({
  'click .phoneSample': function (e, tmpl) {

    var that = this;
    function loadHandler(event) {
      createjs.Sound.play(that.path);
    }

    createjs.Sound.on("fileload", loadHandler, this);
    createjs.Sound.registerSound(this.path, this.path);


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
  'click .sampleSelected': function (e, tmpl) {
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
  }
});

Template.slave.created = function(){
  if( !localStorage.getItem("zouzouId") ){
    localStorage.setItem("zouzouId", Random.hexString(6));
  }
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

Template.slave.destroyed = function(){
  //Meteor.unsubscribe('samples');
};

