extern crate argon2;
extern crate rand;
use argon2::{hash_encoded, Config};
use rand::Rng;

pub struct User {
    pub username: String,
    pub password: Password,
}

#[derive(PartialEq, Eq)]
pub struct Password {
    pub hash: String,
}

#[derive(Debug)]
pub enum Error {
    HashError,
}

impl Password {
    pub fn from_string(plain_password: &str) -> Result<Password, Error> {
        let salt: [u8; 32] = rand::thread_rng().gen();
        let config = Config::default();
        let hash = hash_encoded(plain_password.as_bytes(), &salt, &config)
            .map_err(|_| Error::HashError)?;
        Ok(Password { hash: hash })
    }
}
