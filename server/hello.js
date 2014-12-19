if (Meteor.isClient) {
  // counter starts at 0
  Session.setDefault("counter", 0);

  Template.hello.helpers({
    counter: function () {
      return Session.get("counter");
    }
  });

  Template.hello.events({
    'click button': function () {
      // increment the counter when button is clicked
      Session.set("counter", Session.get("counter") + 1);

      // TEST DDP
      var ddp = new MeteorDdp('ws://localhost:3000/websocket');
      ddp.connect().done(function() {
        console.log('Connected!');
        ddp.call('hiBitch', ["prout"]).done(function(yolo) {
          console.log(yolo);
        });
      });



    }
  });
}

if (Meteor.isServer) {
  Meteor.startup(function () {
    // code to run on server at startup
  });
}
