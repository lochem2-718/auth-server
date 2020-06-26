pub mod config;
pub mod db;
pub mod models;
extern crate log;

extern crate simple_logger;

use config::{Config, LoadError};
use db::Db;
use models::{Password, User};
use std::env;
use std::path::Path;

fn main() {
    // get config
    let args: Vec<String> = env::args().collect();
    let cfg_path = Path::new(args[1].as_str());
    let cfg = match Config::load_from_yaml(cfg_path) {
        Err(e) => {
            match e {
                LoadError::ReadError => {
                    println!("The configuration file {} could not be read", args[1])
                }
                LoadError::SchemaError => {
                    println!("The configuration file {} has an invalid format", args[1])
                }
            };
            std::process::exit(1);
        }
        Ok(cfg) => cfg,
    };

    // setup db
    let mut db = Db::connect(
        cfg.db_url,
        cfg.db_name,
        cfg.db_port,
        cfg.db_username,
        cfg.db_password,
    )
    .unwrap_or_else(|_| {
        println!("A database connection could not be established");
        std::process::exit(1);
    });
    let test_user = User {
        username: String::from("test"),
        password: Password::from_string("password").unwrap(),
    };
    db.add_user(test_user);
    // start db
    // setup server
    // start server
}
