package main

import (
	"unicode/utf8"
	"fmt"
	"net"
)

//yo
func main() {
	//Basic variables
	port := ":8080"
	protocol := "udp"

	//Build the address
	udpAddr, err := net.ResolveUDPAddr(protocol, port)
	if err != nil {
		fmt.Println("Wrong Address")
		return
	}

	//Output
	fmt.Println("\nReading " + protocol + " from " + udpAddr.String())

	//Create the connection
	udpConn, err := net.ListenUDP(protocol, udpAddr)
	if err != nil {
		fmt.Println(err)
	}

	//Keep calling this function
	for {
		display(udpConn)
	}

}

func display(conn *net.UDPConn) {

	var buf [2048]byte
	n, err := conn.Read(buf[0:])
	fmt.Println(n);
	if err != nil {
		fmt.Println("Error Reading")
		return
	} else {
		message := "";
		totSize := 0;
		for i:=0; i<n; i++ {
			r, size := utf8.DecodeRune(buf[i:n])
			message+= string(r)
			totSize += size;
		}
		fmt.Println(message)
		fmt.Println("Package Done")
	}

}
