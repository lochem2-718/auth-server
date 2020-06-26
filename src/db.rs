extern crate postgres;

use crate::models::{Password, User};
use postgres::{Client, NoTls};

pub struct Db {
    client: Client,
}

pub enum Error {
    ConnectError,
    QueryError,
}

impl Db {
    pub fn connect(
        host: String,
        name: String,
        port: u16,
        user: String,
        password: String,
    ) -> Result<Db, Error> {
        let cfg_str = format!(
            "host={} dbname={} port={} user={} password={}",
            host, name, port, user, password
        );
        let mut db = Client::connect(cfg_str.as_str(), NoTls).map_err(|_| Error::ConnectError)?;

        db.simple_query(
            "
        CREATE TABLE IF NOT EXISTS user (
            id SERIAL PRIMARY KEY,
            username VARCHAR(30),
            password VARCHAR(40),
        );",
        )
        .map_err(|_| Error::QueryError)?;
        Ok(Db { client: db })
    }

    pub fn add_user(&mut self, user: User) -> Result<u64, Error> {
        self.client
            .execute(
                "INSERT INTO user (username, password) VALUES ($1, $2)",
                &[&user.username, &user.password.hash],
            )
            .map_err(|_| Error::QueryError)
    }

    pub fn get_user(&mut self, username: String) -> Result<User, Error> {
        let row = self
            .client
            .query_one(
                "SELECT username, password FROM user WHERE username = $1",
                &[&username],
            )
            .map_err(|_| Error::QueryError)?;
        Ok(User {
            username: row.get("username"),
            password: Password {
                hash: row.get("password"),
            },
        })
    }

    pub fn edit_user(&mut self, username: String, new_user: User) -> Result<u64, Error> {
        self.client
            .execute(
                "UPDATE user SET username = $2, password = $3 WHERE username = $1;",
                &[&username, &new_user.username, &new_user.password.hash],
            )
            .map_err(|_| Error::QueryError)
    }

    pub fn remove_user(&mut self, username: String) -> Result<u64, Error> {
        self.client
            .execute("DELETE FROM user WHERE username=?;", &[&username])
            .map_err(|_| Error::QueryError)
    }
}
