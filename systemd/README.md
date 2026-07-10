# `systemd` Services for the NWS Radar Looper


## Publish Images

### Configuration

For the `publish-images.path` file, adjust the watched path to the path where the published images are placed. This should match the `appsettings.json` `Publish:BasePath` value.

###

## Install and Activate `systemd` units

Verify the Syntax of Your Unit Files

You can use the systemd-analyze command with the verify option to check the correctness of your unit files. It will help point out any syntax errors.

```
$ sudo systemd-analyze verify /etc/systemd/system/passwd-mon.*
```

To test the path unit, both of these new units must be activated, so run:

```
$ sudo systemctl enable example.{path,service}
$ sudo systemctl start example.path
```

### Installation

An nice approach to custom systemd services is to use `systemctl link`. It's the equivalent of syslinking the files but you don't have to remember the directory where the services should live.

```
$ sudo systemctl link ~/src/nws-radar-looper/systemd/publish-images.service
$ sudo systemctl link ~/src/nws-radar-looper/systemd/publish-images.path
```

### Enable the service

```
$ sudo systemctl enable --now publish-images.path
```

## Loggging
The primary command is:
> `journalctl -u publish-images.service`

Some useful variations:

Show the most recent entries and follow new ones (like tail -f):
> `journalctl -u publish-images.service -f`

Show only the current boot:
> `journalctl -u publish-images.service -b`

Show the last 50 lines:
> `journalctl -u publish-images.service -n 50`

Show entries since a particular time:
> `journalctl -u publish-images.service --since "10 minutes ago"`

Show only errors and above:
> `journalctl -u publish-images.service -p err`


## Alternatives

The publish image service is an interesting way to handle updating the re-generated images. Another would just to link to the files from the `www` data directory instead.