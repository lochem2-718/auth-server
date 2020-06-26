use crate::config::Config;
use crate::models::{Password, User};
use tokio;
use tokio_postgres::{Client, NoTls};

pub struct Db {
    client: Client,
}

#[derive(Debug)]
pub enum Error {
    ConnectError,
    QueryError,
}

impl Db {
    pub async fn connect(cfg: &Config) -> Result<Db, Error> {
        let cfg_str = format!(
            "host={} dbname={} port={} user={} password={}",
            cfg.db_url, cfg.db_name, cfg.db_port, cfg.db_username, cfg.db_password,
        );
        let (client, conn) = tokio_postgres::connect(&cfg_str, NoTls)
            .await
            .map_err(|_| Error::ConnectError)?;
        tokio::spawn(async move {
            conn.await
                .map_err(|err| eprintln!("connection error: {}", err));
        });
        client
            .simple_query(
                "
        CREATE TABLE IF NOT EXISTS user (
            id SERIAL PRIMARY KEY,
            username VARCHAR(30),
            password VARCHAR(40),
        );",
            )
            .await
            .map_err(|_| Error::QueryError)?;
        Ok(Db { client: client })
    }

    pub async fn add_user(&mut self, user: User) -> Result<u64, Error> {
        self.client
            .execute(
                "INSERT INTO user (username, password) VALUES ($1, $2)",
                &[&user.username, &user.password.hash],
            )
            .await
            .map_err(|_| Error::QueryError)
    }

    pub async fn get_user(&mut self, username: String) -> Result<User, Error> {
        let row = self
            .client
            .query_one(
                "SELECT username, password FROM user WHERE username = $1",
                &[&username],
            )
            .await
            .map_err(|_| Error::QueryError)?;
        Ok(User {
            username: row.get("username"),
            password: Password {
                hash: row.get("password"),
            },
        })
    }

    pub async fn edit_user(&mut self, username: String, new_user: User) -> Result<u64, Error> {
        self.client
            .execute(
                "UPDATE user SET username = $2, password = $3 WHERE username = $1;",
                &[&username, &new_user.username, &new_user.password.hash],
            )
            .await
            .map_err(|_| Error::QueryError)
    }

    pub async fn remove_user(&mut self, username: String) -> Result<u64, Error> {
        self.client
            .execute("DELETE FROM user WHERE username=?;", &[&username])
            .await
            .map_err(|_| Error::QueryError)
    }
}
