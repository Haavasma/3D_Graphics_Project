package udpserver

import (
	"encoding/json"
	"fmt"
	"net"

	"../data"
)

var channels = data.SafeChannelAddressSlice{V: make(map[string][]*net.UDPAddr)}

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
	case "velocity":
		handleVelocity(conn, result, addr)
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

	fmt.Println(addressesToSend)

	bytes, err := json.Marshal(result)
	if err != nil {
		fmt.Println("Can't serialize", result)
	}
	for _, address := range addressesToSend {
		if address != addr {
			conn.WriteTo(bytes, address)
		}
	}
}

func handleVelocity(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {

}

func handlePing(conn *net.UDPConn, result map[string]interface{}, addr *net.UDPAddr) {
	channel, ok := result["channel"].(string)

	if !ok {
		fmt.Println("could not read channel")
	}

	channels.Mux.Lock()
	inChannel := false
	for i := 0; i < len(channels.V[channel]); i++ {
		if addr.IP.Equal(channels.V[channel][i].IP) && channels.V[channel][i].Port == addr.Port {
			inChannel = true
		}
	}
	if !inChannel {
		channels.V[channel] = append(channels.V[channel], addr)
	}
	channels.Mux.Unlock()
}
