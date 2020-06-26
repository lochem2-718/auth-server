pub mod config;
pub mod db;
pub mod models;
use config::Config;
use db::Db;
use models::{Password, User};
use std::env;
use std::path::Path;
use tokio;
use warp;
use warp::{http::Response, Filter};

#[tokio::main]
async fn main() {
    // get config
    let args: Vec<String> = env::args().collect();
    let cfg_path = Path::new(args[1].as_str());
    let cfg = Config::load_from_yaml(cfg_path).expect(format!(
        "There is a problem with the config file {}",
        args[1]
    ));
    // setup db and start db
    let mut db = Db::connect(&cfg)
        .await
        .expect("A database connection could not be established");
    let test_user = User {
        username: String::from("test"),
        password: Password::from_string("password").expect("The password could not be hashed"),
    };
    db.add_user(test_user);
    // setup server

    // start server
    warp::serve(warp::any().map(|| Response::builder().body("neep")));
}
