extern crate serde;
extern crate serde_yaml;

use crate::config::DumpError::*;
use crate::config::LoadError::*;
use serde::{Deserialize, Serialize};
use std::fs::{read_to_string, File};
use std::io::Write;
use std::path::Path;

#[derive(Debug)]
pub enum LoadError {
    ReadError,
    SchemaError,
}
pub enum DumpError {
    WriteError,
}

#[derive(Deserialize, Serialize, Debug)]
pub struct Config {
    pub db_url: String,
    pub db_name: String,
    pub db_port: u16,
    pub db_password: String,
    pub server_port: u16,
    pub db_username: String,
    pub server_secret: String,
}

impl Config {
    pub fn load_from_yaml(path: &Path) -> Result<Config, LoadError> {
        let file = read_to_string(path).map_err(|_| ReadError)?;
        let cfg: Config = serde_yaml::from_str(file.as_str()).map_err(|_| SchemaError)?;
        Ok(cfg)
    }

    pub fn dump_to_yaml(&self, path: &Path) -> Result<(), DumpError> {
        let mut file = File::create(path).map_err(|_| WriteError)?;
        let yaml_str = serde_yaml::to_string(&self).map_err(|_| WriteError)?;
        file.write_all(yaml_str.as_bytes())
            .map_err(|_| WriteError)?;
        Ok(())
    }
}
