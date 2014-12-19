
Router.route('/', function () {
    this.render('home');
});

Router.route('/master', function () {
    this.render('master');
});

Router.route('/slave', function () {
    this.render('slave');
});