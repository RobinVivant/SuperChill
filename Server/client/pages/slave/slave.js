
// counter starts at 0
Session.setDefault("counter", 0);

Template.slave.helpers({
  samples: function () {
    return Samples.find();
  }
});

Template.slave.events({
  'click button': function () {

  }
});

Template.slave.created = function(){
  Meteor.subscribe('samples');

  // SWAG
  setInterval(function(){
    var date = new Date();
    var h = date.getHours();
    var m = date.getMinutes();
    var s = date.getSeconds();
    if((''+h).length == 1) h = '0'+h;
    if((''+s).length == 1) s = '0'+s;
    if((''+m).length == 1) m = '0'+m;

    $('body').velocity({
      properties:{
        backgroundColor: '#'+h+m+s
      }, options:{
        duration:'900'
      }
    });//css('background-color', '#'+h+m+s);
  }, 1000)
};

Template.slave.destroyed = function(){
  Meteor.unsubscribe('samples');
};

