
// counter starts at 0
Session.setDefault("counter", 0);

Template.master.helpers({
  counter: function () {
    return Session.get("counter");
  }
});

Template.master.events({
  'click button': function () {
    // increment the counter when button is clicked
    Session.set("counter", Session.get("counter") + 1);

    // TEST DDP
    /*
    var ddp = new MeteorDdp('ws://superchill.meteor.com/websocket');
    ddp.connect().done(function() {

      console.log('Connected!');

      ddp.subscribe("samples").done(function(){

        console.log('Samples content: ',ddp.getCollection("samples"));

        ddp.watch('samples', function(changedDoc, message) {
          console.log("Samples changed : ", arguments);
        });

      });

      ddp.call('hiBitch', ["prout"]).done(function(yolo) {
        console.log('remote call returns : '+yolo);
      });

    });
    */

  }
});

