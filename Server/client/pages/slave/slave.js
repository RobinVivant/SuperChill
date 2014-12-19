
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
};

Template.slave.destroyed = function(){
  Meteor.unsubscribe('samples');
};

