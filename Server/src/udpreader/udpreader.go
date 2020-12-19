package udpreader

import (
	"encoding/json"
	"fmt"
	"net"
	"time"

	"../data"
)

var channels = data.SafeChannelAddressSlice{V: make(map[string][]data.Pair)}

// ReadUDP Function for reading udpMessage on given connection
func ReadUDP(conn *net.UDPConn) {
	for {
		var buf [1024]byte
		n, addr, err := conn.ReadFromUDP(buf[0:])
		if err != nil {
			fmt.Println("Error Reading")
			return
		}
		go handleMessage(conn, buf, n, addr)
	}
}

func handleMessage(conn *net.UDPConn, buf [1024]byte, n int, addr *net.UDPAddr) {
	var result map[string]interface{}

	json.Unmarshal(buf[0:n], &result)

	switch res := result["type"]; res {
	case "transform":
		handleTransform(conn, result, addr)
	case "ping":
		handlePing(conn, result, addr)
	}
}

func handleTransform(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {
	channel, ok := result["channel"].(string)
	if !ok {
		fmt.Println("could not read channel")
		return
	}
	addressesToSend := channels.V[channel]

	bytes, err := json.Marshal(result)
	if err != nil {
		fmt.Println("Can't serialize", result)
	}
	for _, addressTimePair := range addressesToSend {
		if addressTimePair.Address != addr {
			conn.WriteTo(bytes, addressTimePair.Address)
		}
	}
}

func handlePing(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {
	channel, ok := result["channel"].(string)
	if !ok {
		fmt.Println("could not read channel")
	}
	channels.Mux.Lock()
	inChannel := false
	for i := 0; i < len(channels.V[channel]); i++ {
		if addr.IP.Equal(channels.V[channel][i].Address.IP) && channels.V[channel][i].Address.Port == addr.Port {
			inChannel = true
			channels.V[channel][i].Timestamp = time.Now().Unix()
			break
		}
	}
	if !inChannel {
		channels.V[channel] = append(channels.V[channel], data.Pair{addr, time.Now().Unix()})
	}
	channels.Mux.Unlock()
}

// CheckDCs checks if more than 20 seconds has passed since last ping from client, if so remove channel
func CheckDCs() {
	for {
		fmt.Println("Checking dcs")
		channels.Mux.Lock()
		for channel, addresses := range channels.V {
			for i := 0; i < len(addresses); i++ {
				if time.Now().Unix()-addresses[i].Timestamp > 20 {
					delete(channels.V, channel)
					break
				}
			}
		}
		channels.Mux.Unlock()

		fmt.Println(channels.V)

		time.Sleep(20 * time.Second)
	}
}
